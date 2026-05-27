using UnityEngine;

public class PlayerMove : IState<PlayerController>
{
    private PlayerController m_playerController;
    float speed;
    float elapsedTime;
    Vector2 m_targetPos;

    public void OperateEnter(PlayerController sender)
    {
        if (!m_playerController)
            m_playerController = sender;
        elapsedTime = 0;

        m_playerController.isMoving = true;
        speed = m_playerController.speed;
        m_targetPos = m_playerController.TargetPos;
        m_playerController.AnimationUpdate();

        // ── 핵심 변경 ────────────────────────────────────────
        // 애니메이션이 끝날 때가 아니라 행동을 '결정'한 순간 턴 종료.
        // 적들이 플레이어 애니메이션과 동시에 행동을 시작하므로
        // 대기 시간이 max(플레이어 애니메이션, 적 애니메이션)으로 줄어듦.
        sender.NotifyActionComplete();
        // ─────────────────────────────────────────────────────
    }

    public void OperateUpdate(PlayerController sender)
    {
        Vector2 nowPos = m_playerController.transform.position;
        m_playerController.transform.position = Vector2.Lerp(nowPos, m_targetPos, elapsedTime / speed);

        elapsedTime += Time.deltaTime;
        if (elapsedTime >= speed)
        {
            m_playerController.transform.position = m_targetPos;
        }
    }

    public void OperateExit(PlayerController sender)
    {
        m_playerController.transform.position = m_targetPos;
        m_playerController.isMoving = false;
        m_playerController.AnimationUpdate();
        // TurnManager 직접 참조 제거 — NotifyActionComplete가 Enter에서 이미 처리
    }
}
