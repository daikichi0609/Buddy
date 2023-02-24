using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public readonly struct AttackInfo
{
    /// <summary>
    /// 攻撃者
    /// </summary>
    public Define.CHARA_NAME Attacker { get; }

    /// <summary>
    /// 攻撃力
    /// </summary>
    public int Atk { get; }

    /// <summary>
    /// 命中率
    /// </summary>
    public float Dex { get; }

    public AttackInfo(Define.CHARA_NAME name, int atk, float dex)
    {
        Attacker = name;
        Atk = atk;
        Dex = dex;
    }
}
