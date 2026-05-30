using UnityEngine;

public class MobAtk : IState<MobController>
{
    private MobController m_mobController;
    Vector2 m_Dir;
    float AtkSpeed;
    Vector3 m_OriPos;
    float elapsedTime;
    Vector2 m_targetPos;

    public void OperateEnter(MobController sender)
    {
        if (!m_mobController)
            m_mobController = sender;

        m_targetPos = sender.TargetPos;
        m_Dir = sender.Dir;
        m_OriPos = sender.transform.position;

        // SpeedMultiplier: 달리기 모드면 공격 애니메이션도 빠르게
        // ITurnSystem을 통해 접근 — TurnManager 직접 참조 제거
        float multiplier = ServiceLocator.Get<ITurnSystem>().SpeedMultiplier;
        AtkSpeed = (sender.speed / 2f) / multiplier;

        elapsedTime += Time.deltaTime;
        m_mobController.transform.position = Vector2.Lerp(sender.transform.position, m_targetPos, elapsedTime / AtkSpeed);
    }

    public void OperateUpdate(MobController sender)
    {
        Vector2 nowPos = m_mobController.transform.position;
        m_mobController.transform.position = Vector2.Lerp(nowPos, m_targetPos, elapsedTime / AtkSpeed);

        elapsedTime += Time.deltaTime;
        if (elapsedTime >= AtkSpeed)
        {
            m_mobController.transform.position = m_targetPos;
            m_targetPos = m_OriPos;
            elapsedTime = 0;
        }
    }

    public void OperateExit(MobController sender)
    {
        m_mobController.transform.position = m_OriPos;
        elapsedTime = 0;

        // Controller를 통해 행동 완료 알림 — TurnManager 직접 참조 제거
        sender.OnActionComplete();
    }
}
