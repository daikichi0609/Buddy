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
    public CHARA_NAME Name { get; }

    /// <summary>
    /// 攻撃力
    /// </summary>
    public int Atk { get; }

    /// <summary>
    /// 命中率
    /// </summary>
    public float Dex { get; }

    public AttackInfo(ICollector attacker, CHARA_NAME name, int atk, float dex)
    {
        Attacker = attacker;
        Name = name;
        Atk = atk;
        Dex = dex;
    }
}
