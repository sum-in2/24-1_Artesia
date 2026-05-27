using UnityEngine;

/// <summary>
/// 턴 관리 서비스 인터페이스.
/// State 클래스와 Controller가 TurnManager 싱글톤을 직접 참조하지 않도록 분리합니다.
/// </summary>
public interface ITurnSystem
{
    /// <summary>달리기 모드일 때 애니메이션 배속 (기본 1.0, 달리기 2.0)</summary>
    float SpeedMultiplier { get; }

    /// <summary>플레이어 행동 완료 → 적 턴 시작</summary>
    void EndPlayerTurn();

    /// <summary>특정 오브젝트의 턴 완료 여부를 설정</summary>
    void SetTurn(GameObject obj, bool played);

    /// <summary>달리기 모드 ON/OFF</summary>
    void SetRunMode(bool running);
}
