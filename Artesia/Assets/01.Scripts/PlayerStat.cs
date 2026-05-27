using System.Collections.Generic;
using UnityEngine;

public class PlayerStat : MonoBehaviour, IDamageable
{
    [SerializeField] Dictionary<Stat, List<int>> m_Stat;
    public int NowExp { get; set; }
    public int NowLv { get; set; }
    public int NowHp { get; set; } = 1;
    public int Hp { get; private set; } = 1;
    public int Def { get; private set; }
    public int Atk { get; private set; }
    public int Exp { get; private set; }
    public string Element { get; private set; }

    private void Start()
    {
        if (DataManager.instance != null)
            m_Stat = DataManager.instance.GetCharacterData(this.gameObject.name);

        InitData();
    }

    private void Update()
    {
        if (NowExp >= Exp) LevelUp();
        if (NowHp >= Hp) NowHp = Hp;
        if (NowHp < 0) die();
    }

    public void addHP(int addNum)
    {
        // IBattleLog를 통해 로그 출력 — BattleManager 직접 참조 제거
        if (addNum > 0)
            ServiceLocator.Get<IBattleLog>().AddLog($"레이나의 체력이 {addNum}만큼 회복 되었습니다.");
        else
            ServiceLocator.Get<IBattleLog>().AddLog($"레이나가 {addNum * -1}의 데미지를 입었습니다.");

        NowHp += addNum;
    }

    public void addExp(int addExp)
    {
        NowExp += addExp;
        ServiceLocator.Get<IBattleLog>().AddLog($"레이나의 경험치가 {addExp}만큼 올랐습니다.");
    }

    void InitData()
    {
        Element = "공허";
        NowExp = 0;
        NowLv = 5;
        NowStatSetting();
    }

    void NowStatSetting()
    {
        Hp = m_Stat[Stat.HP][NowLv];
        Def = m_Stat[Stat.DEF][NowLv];
        Atk = m_Stat[Stat.ATK][NowLv];
        Exp = m_Stat[Stat.EXP][NowLv];
        NowHp = Hp;
    }

    void LevelUp()
    {
        ServiceLocator.Get<IBattleLog>().AddLog($"{gameObject.name}의 레벨이 {++NowLv}로 올랐습니다.");
        NowExp = 0;
        NowStatSetting();
    }

    public void SetStat(int savedExp, int savedLV, string name)
    {
        NowExp = savedExp;
        NowLv = savedLV;
        gameObject.name = name;
    }

    public void TakeDamage(int damage)
    {
        // IUIDamageNotifier를 통해 데미지 표시 — UIManager 직접 참조 제거
        ServiceLocator.Get<IUIDamageNotifier>().ShowDamage(gameObject, damage);
        addHP(-1 * damage);
    }

    void die()
    {
        // IUIDamageNotifier를 통해 게임오버 UI — UIManager 직접 참조 제거
        ServiceLocator.Get<IUIDamageNotifier>().ShowGameOver();
        NowStatSetting();
    }
}
