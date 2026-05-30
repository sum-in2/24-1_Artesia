/// <summary>
/// 배틀 로그 출력 서비스 인터페이스.
/// State / Stat 클래스가 BattleManager를 직접 참조하지 않도록 분리합니다.
/// </summary>
public interface IBattleLog
{
    void AddLog(string message);
}
