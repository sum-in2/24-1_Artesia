using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleManager : MonoBehaviour, IBattleLog
{
    public static BattleManager Instance { get; private set; }
    public TMP_Text battleLogText;
    private bool isBattleActive = false;
    private const int maxLines = 4;
    private Queue<string> logLines = new Queue<string>();
    public Canvas dialogCanvas;
    public ScrollRect scrollRect;

    public CanvasStateListener canvasListener;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // ServiceLocator에 등록
        ServiceLocator.Register<IBattleLog>(this);
    }

    private void Start()
    {
        battleLogText.text = "";
        canvasListener.onCanvas.AddListener(ClearBattleLog);

        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        canvasListener.onCanvas.RemoveAllListeners();
    }

    public void startBattle()
    {
        isBattleActive = true;
        dialogCanvas.gameObject.SetActive(true);
    }

    // IBattleLog 구현 — 기존 AddLogMessage를 인터페이스 메서드로 노출
    public void AddLog(string message) => AddLogMessage(message);

    public void AddLogMessage(string message)
    {
        logLines.Enqueue(message);

        if (logLines.Count > maxLines)
        {
            logLines.Dequeue();
        }

        UpdateBattleLog();
    }

    private void UpdateBattleLog()
    {
        if (dialogCanvas.gameObject.activeSelf == false)
        {
            dialogCanvas.gameObject.SetActive(true);
        }

        // 큐로 딜레이 ?
        battleLogText.text = string.Join("\n", logLines);

        if (logLines.Count >= maxLines)
        {
            StartCoroutine(ScrollAndRemoveFirstLine());
        }
    }

    private void ClearBattleLog()
    {
        battleLogText.text = "";
        logLines.Clear();
    }

    IEnumerator ScrollAndRemoveFirstLine()
    {
        Vector3 originalPos = scrollRect.content.localPosition;
        Vector3 targetPos = originalPos + new Vector3(0, 75f);

        float elapsedTime = 0f;
        float duration = 0.2f;

        while (elapsedTime < duration)
        {
            scrollRect.content.localPosition = Vector3.Lerp(originalPos, targetPos, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        scrollRect.content.localPosition = targetPos;

        RemoveFirstLine();

        scrollRect.content.localPosition = originalPos;
    }

    private void RemoveFirstLine()
    {
        string[] lines = battleLogText.text.Split('\n');
        string updatedLog = string.Join("\n", lines, 1, lines.Length - 1);
        battleLogText.text = updatedLog;
    }
}
