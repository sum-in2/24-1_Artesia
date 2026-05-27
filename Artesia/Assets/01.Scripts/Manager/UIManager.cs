using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class UIManager : MonoBehaviour, IUIDamageNotifier
{
    [SerializeField] CanvasGroup LoadingCanvas;
    public bool isFade { get; private set; } = false;
    static UIManager Instance;
    public static UIManager instance
    {
        get
        {
            return Instance;
        }
    }


    public TextMeshProUGUI LoadingStageText;
    public TextMeshProUGUI DungeonInfo;

    bool isBattleLogOn = false;

    Dictionary<string, GameObject> DicUi;
    [SerializeField] GameObject NextStageUI;
    [SerializeField] GameObject statusUI;
    [SerializeField] GameObject gameOverUI;
    [SerializeField] GameObject escapeUI;
    [SerializeField] GameObject optionUI;
    [SerializeField] GameObject keySettingUI;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(this.gameObject);

        DontDestroyOnLoad(gameObject);

        DicUi = new Dictionary<string, GameObject>();
        DicUi.Add("NextStage", NextStageUI);
        DicUi.Add("Status", statusUI);
        DicUi.Add("escape", escapeUI);
        DicUi.Add("option", optionUI);

        // ServiceLocator에 등록
        ServiceLocator.Register<IUIDamageNotifier>(this);
    }

    // ── IUIDamageNotifier 구현 ────────────────────────────────
    public void ShowDamage(GameObject target, int damage) => hit(target, damage);
    public void ShowGameOver() => ShowGameOverUI();
    // ─────────────────────────────────────────────────────────

    public void SetActiveUI(string UIName, bool bActive)
    {
        if (DicUi[UIName] == null)
        {
            Debug.Log("UI가 사전에 업음");
            return;
        }
        DialogOff();
        DicUi[UIName].SetActive(bActive);
    }

    public void SetActiveUI(GameObject UIobj, bool bActive)
    {
        DialogOff();
        UIobj.SetActive(bActive);
    }

    private void DialogOff()
    {
        Canvas dialogCanvas = BattleManager.Instance?.dialogCanvas;
        if (dialogCanvas != null && dialogCanvas.gameObject.activeSelf)
            dialogCanvas.gameObject.SetActive(false);
    }

    public void ShowGameOverUI()
    {
        Time.timeScale = 0f;
        DialogOff();
        gameOverUI.SetActive(true);
    }

    public IEnumerator FakeLoading(float time, int stageIndex, string dungeonName)
    {
        Debug.Log(dungeonName);
        isFade = true;
        float timer = 0f;
        LoadingCanvas.alpha = 1f;

        if (LoadingStageText != null)
            LoadingStageText.text = dungeonName + "\n" + stageIndex + "F";

        yield return new WaitForSeconds(time);

        while (timer < time)
        {
            yield return null;
            timer += Time.deltaTime;
            LoadingCanvas.alpha = Mathf.Lerp(1f, 0f, timer / time);
        }

        isFade = false;
    }

    public void SetDungeonInfoText(string DungeonName, int stageIndex)
    {
        DungeonInfo.text = DungeonName + " " + stageIndex + "F";
    }

    public void SetDungeonInfoText(string DungeonName)
    {
        DungeonInfo.text = DungeonName;
    }

    public void hit(GameObject obj, int Dmg)
    {
        GameObject DmgText = Resources.Load<GameObject>("Prefabs/DmgText");
        if (DmgText == null)
        {
            Debug.Log("dmgtext 로드 실패");
            return;
        }
        GameObject textObj = Instantiate(DmgText);
        textObj.GetComponent<DmgText>().Init(Dmg, obj.transform.position);
    }
}
