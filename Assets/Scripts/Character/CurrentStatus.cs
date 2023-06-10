using System;
using NaughtyAttributes;
using System.Collections.Generic;
using UniRx;

[Serializable]
public class CurrentStatus
{
    /// <summary>
    /// バフ
    /// </summary>
    private List<BuffTicket> m_BuffList = new List<BuffTicket>();

    /// <summary>
    /// 初期化
    /// </summary>
    /// <param name="param"></param>
    public CurrentStatus(BattleStatus.Parameter param, int lv)
    {
        OriginParam = param;
        Lv = lv;
        Hp = OriginParam.MaxHp;
    }

    // 元ステータス
    [ShowNativeProperty]
    public BattleStatus.Parameter OriginParam { get; }

    // レベル
    [ShowNativeProperty]
    public int Lv { get; set; }
    private float LvMag => 1f + Lv * 0.1f;

    // ヒットポイント
    [ShowNativeProperty]
    public int Hp { get; set; }
    public int MaxHp => (int)(OriginParam.MaxHp * LvMaxHpMag);
    private float LvMaxHpMag => 1f + Lv * 0.01f;

    // 攻撃力
    [ShowNativeProperty]
    public int Atk => (int)(OriginParam.Atk * LvAtkMag * BuffAtkMag);
    private float LvAtkMag => 1f + Lv * 0.1f;
    public float BuffAtkMag
    {
        get
        {
            float mag = 1f;
            foreach (var buff in m_BuffList)
                mag += buff.AtkMag;

            return mag;
        }
    }

    // 防御力
    [ShowNativeProperty]
    public int Def => (int)(OriginParam.Def * LvDefMag * BuffDefMag);
    private float LvDefMag => 1f + Lv * 0.1f;
    public float BuffDefMag
    {
        get
        {
            float mag = 1f;
            foreach (var buff in m_BuffList)
                mag += buff.DefMag;

            return mag;
        }
    }

    // 速さ
    [ShowNativeProperty]
    public int Agi { get; set; }

    // 抵抗率
    [ShowNativeProperty]
    public float Res { get; set; }

    /// <summary>
    /// バフを付与する
    /// </summary>
    /// <param name="buff"></param>
    /// <returns></returns>
    public IDisposable AddBuff(BuffTicket buff)
    {
        m_BuffList.Add(buff);
        return Disposable.CreateWithState((m_BuffList, buff), tuple =>
        {
            tuple.m_BuffList.Remove(tuple.buff);
        });
    }
}

public readonly struct BuffTicket
{
    public float AtkMag { get; }
    public float DefMag { get; }

    public BuffTicket(float atkMag, float defMag)
    {
        AtkMag = atkMag;
        DefMag = defMag;
    }
}