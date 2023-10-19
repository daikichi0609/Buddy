using System;
using NaughtyAttributes;
using System.Collections.Generic;
using UniRx;

[Serializable]
public class CurrentStatus
{
    /// <summary>
    /// 元セットアップ
    /// </summary>
    [ShowNativeProperty]
    public CharacterSetup Setup { get; }

    /// <summary>
    /// 元ステータス
    /// </summary>
    [ShowNativeProperty]
    public BattleStatus.Parameter OriginParam { get; }

    /// <summary>
    /// バフ
    /// </summary>
    private List<BuffTicket> m_BuffList = new List<BuffTicket>();

    // レベル
    [ShowNativeProperty]
    public int Lv { get; set; }
    private float LvMag => 1f + Lv * 0.05f;
    public bool IsDead => Hp == 0;

    // ヒットポイント
    [ShowNativeProperty]
    public int Hp { get; private set; }
    public int MaxHp => (int)(OriginParam.MaxHp * LvMaxHpMag);
    private float LvMaxHpMag => 1f + LvMag * 0.01f;

    // 攻撃力
    [ShowNativeProperty]
    public int Atk => (int)(OriginParam.Atk * LvMag * GetBuffMag(PARAMETER_TYPE.ATK));

    // 防御力
    [ShowNativeProperty]
    public int Def => (int)(OriginParam.Def * LvMag * GetBuffMag(PARAMETER_TYPE.DEF));

    /// <summary>
    /// クリティカル上昇率
    /// </summary>
    public float Cr => 1f * GetBuffMag(PARAMETER_TYPE.CR);

    /// <summary>
    /// 初期化
    /// </summary>
    /// <param name="param"></param>
    public CurrentStatus(CharacterSetup setup, BattleStatus.Parameter param, int lv)
    {
        Setup = setup;
        OriginParam = param;
        Lv = lv;
        Hp = MaxHp;
    }

    /// <summary>
    /// 回復、ダメージメソッド
    /// </summary>
    /// <param name="add"></param>
    /// <param name="canDead"></param>
    public int RecoverHp(int add, bool canDead = true)
    {
        int min = canDead ? 0 : 1;
        int prevHp = Hp;
        Hp = Math.Clamp(Hp + add, min, MaxHp);
        return Hp - prevHp;
    }

    /// <summary>
    /// バフ倍率取得
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private float GetBuffMag(PARAMETER_TYPE type)
    {
        float mag = 1f;
        foreach (var buff in m_BuffList)
            if (buff.Type == type)
                mag += buff.Mag;
        return mag;
    }

    /// <summary>
    /// バフを付与する
    /// </summary>
    /// <param name="buff"></param>
    /// <returns></returns>
    public IDisposable AddBuff(BuffTicket buff)
    {
        m_BuffList.Add(buff);
        return Disposable.CreateWithState((m_BuffList, buff), tuple => tuple.m_BuffList.Remove(tuple.buff));
    }
}

public enum PARAMETER_TYPE
{
    ATK,
    DEF,
    CR,
}

public readonly struct BuffTicket
{
    public PARAMETER_TYPE Type { get; }
    public float Mag { get; }

    public BuffTicket(PARAMETER_TYPE type, float mag)
    {
        Type = type;
        Mag = mag;
    }
}

