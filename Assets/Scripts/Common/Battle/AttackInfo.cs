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
    /// 防御無視
    /// </summary>
    public bool IgnoreDefence { get; }

    /// <summary>
    /// 攻撃方向
    /// </summary>
    public DIRECTION Direction { get; }

    public AttackInfo(ICollector attacker, string name, int atk, float dex, bool ignoreDeffence, DIRECTION dir)
    {
        Attacker = attacker;
        Name = name;
        Atk = atk;
        Dex = dex;
        IgnoreDefence = ignoreDeffence;
        Direction = dir;
    }
}
