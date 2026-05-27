using System.Collections;
using UnityEngine;

public class MobStat : MonoBehaviour, IDamageable
{
    int HP;
    int DEF;
    public int ATK;
    int EXP;
    private SpriteRenderer spriteRenderer;
    bool isFade = false;

    public bool isDead = false;

    private void OnEnable()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        Color color = spriteRenderer.color;
        color.a = 1f;
        spriteRenderer.color = color;

        HP = 25;
        ATK = 5;
        EXP = 8;

        isDead = false;
    }

    private void Update()
    {
        if (HP < 0 && !isFade)
            die();
    }

    public void TakeDamage(int damage)
    {
        // UIManager / BattleManager 직접 참조 제거 → ServiceLocator 경유
        ServiceLocator.Get<IUIDamageNotifier>().ShowDamage(gameObject, damage);
        ServiceLocator.Get<IBattleLog>().AddLog($"보이드 리퍼가 {damage}의 데미지를 입었습니다.");
        HP -= damage;
    }

    public void die()
    {
        GameObject obj = GameObject.FindGameObjectWithTag("Player");

        isDead = true;

        obj.GetComponent<PlayerStat>().addExp(EXP);

        StartCoroutine(DieFade());
    }

    private IEnumerator DieFade()
    {
        Color color = spriteRenderer.color;
        float fadeDuration = 0.5f;
        isFade = true;

        while (color.a > 0)
        {
            color.a -= Time.deltaTime / fadeDuration;
            spriteRenderer.color = color;
            yield return null;
        }

        isFade = false;

        // EnemySpawner 직접 참조 제거 → ServiceLocator 경유
        ServiceLocator.Get<IEnemyRegistry>().KillEnemy(gameObject);
    }
}
