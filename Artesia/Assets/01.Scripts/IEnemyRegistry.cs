using UnityEngine;

/// <summary>
/// 적 등록/제거 서비스 인터페이스.
/// MobStat이 EnemySpawner를 직접 참조하지 않도록 분리합니다.
/// </summary>
public interface IEnemyRegistry
{
    void KillEnemy(GameObject enemy);
}
