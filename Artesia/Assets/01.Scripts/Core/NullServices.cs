using UnityEngine;

// ──────────────────────────────────────────────────────────────
// 테스트 씬 / Manager 없는 환경에서 ServiceLocator가 자동으로
// 반환하는 Null 구현체들입니다.
// 실제 Manager가 등록되면 이 구현체들은 사용되지 않습니다.
// ──────────────────────────────────────────────────────────────

public class NullTurnSystem : ITurnSystem
{
    public float SpeedMultiplier => 1f;
    public void EndPlayerTurn() { }
    public void SetTurn(GameObject obj, bool played) { }
    public void SetRunMode(bool running) { }
}

public class NullBattleLog : IBattleLog
{
    // 테스트 씬에서도 로그는 확인할 수 있도록 Debug.Log 출력
    public void AddLog(string message) => Debug.Log($"[BattleLog] {message}");
}

public class NullUIDamageNotifier : IUIDamageNotifier
{
    public void ShowDamage(GameObject target, int damage) =>
        Debug.Log($"[Damage] {target.name} -{damage}HP");
    public void ShowGameOver() =>
        Debug.Log("[GameOver]");
}

public class NullEnemyRegistry : IEnemyRegistry
{
    public void KillEnemy(GameObject enemy) =>
        Debug.Log($"[EnemyDied] {enemy.name}");
}
