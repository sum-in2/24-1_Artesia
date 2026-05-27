using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using System;

public class TurnManager : MonoBehaviour
{
    GameObject Player;
    public int spawnTurn = 8;
    List<GameObject> MobList;

    static TurnManager Instance;
    public static TurnManager instance
    {
        get
        {
            return Instance;
        }
    }
    String sceneName;

    int TurnCnt;

    // 달리기 모드일 때 몬스터 애니메이션 속도를 빠르게 (1 = 기본, 2 = 2배속)
    public float SpeedMultiplier { get; private set; } = 1f;

    public void SetRunMode(bool running)
    {
        SpeedMultiplier = running ? 2f : 1f;
    }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(this.gameObject);

        TurnCnt = 0;
        sceneName = SceneManager.GetActiveScene().name;
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
            setTurn(Player, false);

        if (TurnCnt > spawnTurn && MobList != null)
        {
            EnemySpawner.instance.RandomSpawnEnemy();
            TurnCnt = 0;
        }
    }

    public void EndPlayerTurn()
    {
        Player.GetComponent<ITurn>().PlayedTurn = true;
        if (sceneName != "BaseCamp")
            EnemyNextTurn();
    }

    void EnemyNextTurn()
    {
        foreach (GameObject Obj in MobList)
        {
            setTurn(Obj, false);
            EnemySpawner.instance.updatePath(Obj, Player.transform.position);
        }
        TurnCnt++;
    }

    public void setTurn(GameObject obj, bool input)
    {
        ITurn TurnTemp = obj.GetComponent<ITurn>();
        if (TurnTemp != null)
        {
            TurnTemp.PlayedTurn = input;
        }
        else
            Debug.Log("ITurn 상속받지 않은 오브젝트 : " + obj.name);
    }

    bool CheckUnitTurn()
    {
        if (MobList != null)
        {
            foreach (GameObject Obj in MobList)
            {
                if (!Obj.activeSelf)
                {
                    continue;
                }

                if (!Obj.GetComponent<ITurn>().PlayedTurn)
                {
                    return false;
                }
            }
        }
        return true;
    }
}
