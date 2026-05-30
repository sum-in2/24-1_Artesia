using System.Collections.Generic;
using UnityEngine;

public class PlayerAtk : IState<PlayerController>
{
    Vector2 m_Dir;
    float AtkSpeed;
    private PlayerController m_playerController;
    float elapsedTime;

    Vector2 atkCenter;
    Vector2 atkSize;
    int atkDamage;

    public void OperateEnter(PlayerController sender)
    {
        if (!m_playerController)
            m_playerController = sender;
        initStat();

        atkCenter = (Vector2)sender.transform.position + m_Dir * 0.5f;

        // IBattleLog를 통해 로그 출력 — BattleManager 직접 참조 제거
        ServiceLocator.Get<IBattleLog>().AddLog("레이나 일반공격 사용!");

        normalAtk();

        elapsedTime += Time.deltaTime;

        // 데미지가 이미 적용됐으므로 즉시 턴 종료
        sender.NotifyActionComplete();
    }

    public void OperateUpdate(PlayerController sender)
    {
        elapsedTime += Time.deltaTime;
        if (elapsedTime >= AtkSpeed)
        {
            sender.EnemyHit = false;
        }
    }

    public void OperateExit(PlayerController sender)
    {
        elapsedTime = 0;
        // TurnManager 직접 참조 제거 — NotifyActionComplete가 Enter에서 이미 처리
    }

    void normalAtk()
    {
        atkSize = new Vector2(0.5f, 0.5f);
        Collider2D[] others = Physics2D.OverlapBoxAll(atkCenter, atkSize, 0);

        foreach (Collider2D collider in others)
        {
            if (collider.CompareTag("Enemy"))
            {
                collider.gameObject.GetComponent<MobStat>().TakeDamage(atkDamage);
            }
        }
    }

    void initStat()
    {
        m_Dir = m_playerController.Dir;
        AtkSpeed = m_playerController.speed / 2f;
        atkDamage = m_playerController.GetComponent<PlayerStat>().Atk;
    }
}
