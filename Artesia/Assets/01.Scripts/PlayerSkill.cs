using UnityEngine;

public class PlayerSkill : IState<PlayerController>
{
    private PlayerController m_playerController;
    public int damage = 15;
    public float skillRange = 1f;
    public float skillDelay = 0.3f;
    string skillName = "칼날 회오리";

    private float delayTimer;

    public void OperateEnter(PlayerController sender)
    {
        if (!m_playerController)
            m_playerController = sender;

        // IBattleLog를 통해 로그 출력 — BattleManager 직접 참조 제거
        ServiceLocator.Get<IBattleLog>().AddLog($"레이나 {skillName} 사용!");

        ActivateSkill();
        delayTimer = 0f;

        // 스킬 효과가 즉시 적용됐으므로 즉시 턴 종료
        sender.NotifyActionComplete();
    }

    public void OperateUpdate(PlayerController sender)
    {
        delayTimer += Time.deltaTime;
        if (delayTimer >= skillDelay)
        {
            sender.isSkillActive = false;
        }
    }

    public void OperateExit(PlayerController sender)
    {
        // TurnManager 직접 참조 제거 — NotifyActionComplete가 Enter에서 이미 처리
    }

    private void ActivateSkill()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(m_playerController.transform.position, skillRange);

        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Enemy"))
            {
                collider.GetComponent<IDamageable>().TakeDamage(damage);
            }
        }
    }
}
