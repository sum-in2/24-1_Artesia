using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using System;

public class TurnManager : MonoBehaviour, ITurnSystem
{
    GameObject Player;
    public int spawnTurn = 8;
    List<GameObject> MobList;

    static TurnManager Instance;
    public static TurnManager instance
    {
        get { return Instance; }
    }
    String sceneName;
    int TurnCnt;

    // ── ITurnSystem ───────────────────────────────────────────
    /// <summary>달리기 모드일 때 몬스터 애니메이션 배속 (기본 1, 달리기 2)</summary>
    public float SpeedMultiplier { get; private set; } = 1f;

    public void SetRunMode(bool running)
    {
        SpeedMultiplier = running ? 2f : 1f;
    }

    public void EndPlayerTurn()
    {
        Player.GetComponent<ITurn>().PlayedTurn = true;
        if (sceneName != "BaseCamp")
            EnemyNextTurn();
    }

    public void SetTurn(GameObject obj, bool played)
    {
        ITurn TurnTemp = obj.GetComponent<ITurn>();
        if (TurnTemp != null)
            TurnTemp.PlayedTurn = played;
        else
            Debug.Log("ITurn 상속받지 않은 오브젝트 : " + obj.name);
    }
    // ─────────────────────────────────────────────────────────

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(this.gameObject);

        TurnCnt = 0;
        sceneName = SceneManager.GetActiveScene().name;

        // ServiceLocator에 등록 — State/Stat 클래스들이 이 인터페이스로 접근
        ServiceLocator.Register<ITurnSystem>(this);
    }

    private void Start()
    {
        if (sceneName != "BaseCamp")
            MobList = EnemySpawner.instance.enemies;

        Player = GameObject.FindGameObjectWithTag("Player");
    }

    private void Update()
    {
        if (CheckUnitTurn())
            SetTurn(Player, false);

        if (TurnCnt > spawnTurn && MobList != null)
        {
            EnemySpawner.instance.RandomSpawnEnemy();
            TurnCnt = 0;
        }
    }

    void EnemyNextTurn()
    {
        // 플레이어의 '논리적 위치'(TargetPos)로 경로 계산
        // → EndPlayerTurn이 OperateEnter에서 호출되므로
        //   transform.position은 아직 이전 칸이지만 TargetPos는 이미 다음 칸을 가리킴
        PlayerController pc = Player.GetComponent<PlayerController>();
        Vector2 playerLogical = pc != null ? pc.TargetPos : (Vector2)Player.transform.position;

        foreach (GameObject Obj in MobList)
        {
            SetTurn(Obj, false);
            EnemySpawner.instance.updatePath(Obj, playerLogical);
        }
        TurnCnt++;
    }

    bool CheckUnitTurn()
    {
        if (MobList != null)
        {
            foreach (GameObject Obj in MobList)
            {
                if (!Obj.activeSelf) continue;
                if (!Obj.GetComponent<ITurn>().PlayedTurn) return false;
            }
        }
        return true;
    }
}
