using UnityEngine;

public class MobMove : IState<MobController>
{
    private MobController m_mobController;
    float m_speed;
    Vector2 m_targetPos;
    float elapsedTime;

    public void OperateEnter(MobController sender)
    {
        if (!m_mobController)
            m_mobController = sender;
        elapsedTime = 0;

        // SpeedMultiplier: 달리기 모드면 적 애니메이션도 빠르게
        // ITurnSystem을 통해 접근 — TurnManager 직접 참조 제거
        float multiplier = ServiceLocator.Get<ITurnSystem>().SpeedMultiplier;
        m_speed = m_mobController.speed / multiplier;
        m_targetPos = m_mobController.TargetPos;
    }

    public void OperateUpdate(MobController sender)
    {
        Vector3 nowPos = m_mobController.transform.position;
        m_mobController.transform.position = Vector2.Lerp(nowPos, m_targetPos, elapsedTime / m_speed);
        elapsedTime += Time.deltaTime;
        if (elapsedTime >= m_speed)
        {
            m_mobController.transform.position = m_targetPos;
        }
    }

    public void OperateExit(MobController sender)
    {
        m_mobController.transform.position = m_targetPos;

        // Controller를 통해 행동 완료 알림 — TurnManager 직접 참조 제거
        sender.OnActionComplete();
    }
}
