using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public readonly struct AttackResult
{
    /// <summary>
    /// 攻撃者
    /// </summary>
    public AttackInfo AttackInfo { get; }

    /// <summary>
    /// 被攻撃者
    /// </summary>
    public ICollector Defender { get; }

    /// <summary>
    /// 被攻撃者の名前
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// ヒットしたかどうか
    /// </summary>
    public bool IsHit { get; }

    /// <summary>
    /// 与えたダメージ
    /// </summary>
    public int Damage { get; }

    /// <summary>
    /// 相手の残りHp
    /// </summary>
    public int RemainingHp { get; }

    /// <summary>
    /// 相手は死亡したか
    /// </summary>
    public bool IsDead { get; }

    public AttackResult(AttackInfo info, ICollector defender, bool isHit, int damage, int remainingHp, bool isDead)
    {
        AttackInfo = info;
        Defender = defender;
        Name = Defender.GetInterface<ICharaStatus>().CurrentStatus.Name;
        IsHit = isHit;
        Damage = damage;
        RemainingHp = remainingHp;
        IsDead = isDead;
    }

    public static readonly AttackResult Invalid = new AttackResult();
}