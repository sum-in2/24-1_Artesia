using UnityEngine;

/// <summary>
/// 데미지 표시 / 게임오버 UI 서비스 인터페이스.
/// Stat 클래스가 UIManager를 직접 참조하지 않도록 분리합니다.
/// </summary>
public interface IUIDamageNotifier
{
    void ShowDamage(GameObject target, int damage);
    void ShowGameOver();
}
