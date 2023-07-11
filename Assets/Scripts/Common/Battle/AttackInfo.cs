using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public readonly struct AttackInfo
{
    /// <summary>
    /// 攻撃者
    /// </summary>
    public ICollector Attacker { get; }

    /// <summary>
    /// 攻撃者（名前）
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 攻撃力
    /// </summary>
    public int Atk { get; }

    /// <summary>
    /// 命中率
    /// </summary>
    public float Dex { get; }

    /// <summary>
    /// 急所確率
    /// </summary>
    public float CriticalRatio { get; }

    /// <summary>
    /// 防御無視
    /// </summary>
    public bool IgnoreDefence { get; }

    /// <summary>
    /// 攻撃方向
    /// </summary>
    public DIRECTION Direction { get; }

    public AttackInfo(ICollector attacker, string name, int atk, float dex, float cr, bool ignoreDeffence, DIRECTION dir)
    {
        Attacker = attacker;
        Name = name;
        Atk = atk;
        Dex = dex;
        CriticalRatio = cr;
        IgnoreDefence = ignoreDeffence;
        Direction = dir;
    }
}

public readonly struct AttackPercentageInfo
{
    /// <summary>
    /// 攻撃者
    /// </summary>
    public ICollector Attacker { get; }

    /// <summary>
    /// 攻撃者（名前）
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 攻撃力
    /// </summary>
    public float Ratio { get; }

    /// <summary>
    /// 命中率
    /// </summary>
    public float Dex { get; }

    /// <summary>
    /// 攻撃方向
    /// </summary>
    public DIRECTION Direction { get; }

    public AttackPercentageInfo(ICollector attacker, string name, float ratio, float dex, DIRECTION dir)
    {
        Attacker = attacker;
        Name = name;
        Ratio = ratio;
        Dex = dex;
        Direction = dir;
    }
}

public readonly struct AttackFixedInfo
{
    /// <summary>
    /// 攻撃者
    /// </summary>
    public ICollector Attacker { get; }

    /// <summary>
    /// 攻撃者（名前）
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 攻撃力
    /// </summary>
    public int Damage { get; }

    /// <summary>
    /// 命中率
    /// </summary>
    public float Dex { get; }

    /// <summary>
    /// 攻撃方向
    /// </summary>
    public DIRECTION Direction { get; }

    public AttackFixedInfo(ICollector attacker, string name, int damage, float dex, DIRECTION dir)
    {
        Attacker = attacker;
        Name = name;
        Damage = damage;
        Dex = dex;
        Direction = dir;
    }
}