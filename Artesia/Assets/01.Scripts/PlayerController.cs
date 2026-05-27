using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour, ITurn
{
    public enum PlayerState
    {
        Idle,
        Move,
        Atk,
        Skill,
    }

    private Dictionary<PlayerState, IState<PlayerController>> dicState = new Dictionary<PlayerState, IState<PlayerController>>();
    private StateMachine<PlayerController> SM;
    public Vector2 OriPos { get; private set; }
    public Vector2 Dir { get; set; } = Vector2.down;
    public Vector2 TargetPos { get; private set; }
    [SerializeField][Range(0.0001f, 1f)][Tooltip("커질수록 느려짐")] float Speed = 0.2f;
    public float speed
    {
        get { return Speed; }
    }
    public bool isMoving { get; set; } = false;
    public bool isSkillActive { get; set; } = false;
    public bool PlayedTurn { get; set; }
    public bool EnemyHit { get; set; } = false;

    public string destroySceneName;

    // ── 달리기 관련 ──────────────────────────────────────
    // Shift 키를 누르고 있는 동안 달리기 모드
    private bool _isRunHeld = false;
    // 달리기 중 자동으로 이동할 방향 (마지막으로 누른 방향키)
    private Vector2 _runDir = Vector2.zero;
    // ────────────────────────────────────────────────────

    // ── 입력 버퍼 ────────────────────────────────────────
    // 애니메이션 중이거나 적 턴 대기 중에 눌린 방향 입력을 보관.
    // 행동 가능 상태가 되면 즉시 소비합니다.
    private Vector2 _pendingInput = Vector2.zero;
    // ────────────────────────────────────────────────────

    private void Awake()
    {
        IState<PlayerController> idle = new PlayerIdle();
        IState<PlayerController> move = new PlayerMove();
        IState<PlayerController> atk = new PlayerAtk();
        IState<PlayerController> skill = new PlayerSkill();

        dicState.Add(PlayerState.Idle, idle);
        dicState.Add(PlayerState.Move, move);
        dicState.Add(PlayerState.Atk, atk);
        dicState.Add(PlayerState.Skill, skill);


        SM = new StateMachine<PlayerController>(this, dicState[PlayerState.Idle]);
    }

    void Start()
    {
        MovePos();
        DontDestroyOnLoad(gameObject);
        if (GameObject.FindGameObjectsWithTag("Player").Length > 1)
            Destroy(gameObject);
        UIManager.instance?.SetActiveUI("Status", true);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().name == destroySceneName)
            Destroy(gameObject);

        if ((Vector2)transform.position == TargetPos && SM.CurState == dicState[PlayerState.Move])
            SM.SetState(dicState[PlayerState.Idle]);

        if (!EnemyHit && SM.CurState == dicState[PlayerState.Atk])
            SM.SetState(dicState[PlayerState.Idle]);

        if (SM.CurState == dicState[PlayerState.Skill] && !isSkillActive)
        {
            SM.SetState(dicState[PlayerState.Idle]);
        }

        if (Mathf.Abs(Dir.x) == 1)
        {
            gameObject.GetComponent<SpriteRenderer>().flipX = (Dir.x == 1);
        }

        // ── 달리기 처리 ──────────────────────────────────
        UpdateRunInput();
        // ─────────────────────────────────────────────────

        SM.DoOperateUpdate();
    }

    // Shift 키 상태를 매 프레임 체크하고, 달리기/입력버퍼 자동이동을 처리
    private void UpdateRunInput()
    {
        bool shiftNow = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;

        // Shift 상태가 바뀌었을 때만 처리
        if (shiftNow != _isRunHeld)
        {
            _isRunHeld = shiftNow;
            if (!_isRunHeld)
                StopRun(); // Shift 뗐으면 달리기 중단
        }

        bool canAct = !PlayedTurn
                      && SM.CurState == dicState[PlayerState.Idle]
                      && !(UIManager.instance?.isFade ?? false);

        if (!canAct) return;

        // 우선순위 1: 버퍼된 입력 소비
        // (애니메이션 중 또는 적 턴 대기 중에 눌렸던 방향키)
        if (_pendingInput != Vector2.zero)
        {
            Vector2 pending = _pendingInput;
            _pendingInput = Vector2.zero;
            TryMove(pending);
            return;
        }

        // 우선순위 2: 달리기 자동 이동
        if (_isRunHeld && _runDir != Vector2.zero)
        {
            TryMove(_runDir);
        }
    }

    // 달리기 중단: 방향 초기화 + 몬스터 속도 배율 복원
    private void StopRun()
    {
        _runDir = Vector2.zero;
        ServiceLocator.Get<ITurnSystem>().SetRunMode(false);
    }

    /// <summary>
    /// State 클래스(PlayerMove/PlayerAtk/PlayerSkill)가
    /// 행동 완료를 알릴 때 호출합니다.
    /// State는 TurnManager를 모르고, Controller를 통해 ServiceLocator로 위임합니다.
    /// </summary>
    public void NotifyActionComplete()
    {
        ServiceLocator.Get<ITurnSystem>().EndPlayerTurn();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "BaseCamp")
        {
            transform.position = Vector3.zero;
        }
    }

    public void AnimationUpdate()
    {
        Animator animator;

        animator = GetComponent<Animator>();

        animator.SetFloat("DirX", Dir.x);
        animator.SetFloat("DirY", Dir.y);
        animator.SetBool("isMoving", isMoving);
    }

    void OnMove(InputValue value)
    {
        Vector2 input = value.Get<Vector2>();

        if (input != Vector2.zero)
        {
            _runDir = input; // 달리기 방향 갱신 (항상)

            if (SM.CurState == dicState[PlayerState.Idle]
                && !PlayedTurn
                && !(UIManager.instance?.isFade ?? false))
            {
                // 즉시 이동 가능한 상태
                TryMove(input);
            }
            else
            {
                // 애니메이션 중이거나 적 턴 대기 중이면 버퍼에 저장
                // → UpdateRunInput에서 행동 가능 시점에 소비됨
                _pendingInput = input;
            }
        }
        else
        {
            // 방향키를 뗐으면 달리기/버퍼 모두 초기화
            _runDir = Vector2.zero;
            _pendingInput = Vector2.zero;
        }
    }

    // OnMove와 자동달리기 양쪽에서 공통으로 사용하는 이동 시도 로직
    private void TryMove(Vector2 input)
    {
        DirControl(input);
        Vector3Int currentPosInt = new Vector3Int((int)transform.position.x, (int)transform.position.y, 0);
        RaycastHit2D hit = Physics2D.Raycast(
            (Vector2Int)currentPosInt, input, 1,
            LayerMask.GetMask("Tile") | LayerMask.GetMask("Enemy")
        );

        if (!hit)
        {
            TargetPos = currentPosInt + new Vector3(Dir.x, Dir.y, 0);

            // 달리기 중이면 몬스터 속도 배율 활성화
            if (_isRunHeld)
                ServiceLocator.Get<ITurnSystem>().SetRunMode(true);

            SM.SetState(dicState[PlayerState.Move]);
        }
        else if (_isRunHeld)
        {
            // 달리는 중 벽이나 적에 막히면 달리기 중단
            StopRun();
        }
    }

    void OnSkill(InputValue value)
    {
        if (!PlayedTurn && SM.CurState == dicState[PlayerState.Idle]
            && !(UIManager.instance?.isFade ?? false))
        {
            SM.SetState(dicState[PlayerState.Skill]);
            isSkillActive = true;
        }
    }

    void OnOption(InputValue value)
    {
        UIManager.instance?.SetActiveUI("option", true);
        if (SceneManager.GetActiveScene().name != "BaseCamp")
            UIManager.instance?.SetActiveUI("escape", true);
        Time.timeScale = 0f;
    }

    void OnAtk(InputValue value)
    {
        if (!PlayedTurn && SM.CurState == dicState[PlayerState.Idle]
            && !(UIManager.instance?.isFade ?? false))
        {
            EnemyHit = true;
            SM.SetState(dicState[PlayerState.Atk]);
        }
    }

    public void DirControl(Vector2 dir)
    {
        Vector2 Result = dir;
        float x, y;
        x = Mathf.Abs(Result.x);
        y = Mathf.Abs(Result.y);

        if ((x + y) > 1)
        {
            Result.x = Result.x > 0 ? Mathf.CeilToInt(Result.x) : Mathf.FloorToInt(Result.x);
            Result.y = Result.y > 0 ? Mathf.CeilToInt(Result.y) : Mathf.FloorToInt(Result.y);
        }

        Dir = Result;
        AnimationUpdate();
    }

    public void MovePos()
    {
        SM.SetState(dicState[PlayerState.Idle]);

        if (MapGenerator.instance != null)
            transform.position = MapGenerator.instance.StartPos;
        else
            transform.position = Vector2.zero;
    }

    public void MovePos(Vector3 pos)
    {
        SM.SetState(dicState[PlayerState.Idle]);
        transform.position = pos;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube((Vector2)transform.position + Dir, new Vector2(0.5f, 0.5f));
    }
}
