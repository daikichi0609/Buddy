using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BattleSystem
{
    private static readonly float CR_DAMAGE_UP_RATIO = 1.5f;

    public delegate AttackInfo OnDamageEvent(AttackInfo info, ICollector defender);

    /// <summary>
    /// ダメージ
    /// </summary>
    /// <param name="info"></param>
    /// <param name="defender"></param>
    /// <returns></returns>
    public static AttackResult Damage(AttackInfo info, ICollector defender)
    {
        if (info.Attacker.RequireEvent<ICharaBattleEvent>(out var battleEvent) == true && battleEvent.OnDamageEvent != null)
            info = battleEvent.OnDamageEvent.Invoke(info, defender);

        var defenderStatus = defender.GetInterface<ICharaStatus>().CurrentStatus;

        // ヒット判定
        float random = UnityEngine.Random.Range(0, 1f);
        bool isHit = info.Dex >= random;

        // 急所判定
        random = UnityEngine.Random.Range(0, 1f);
        bool isCritical = info.CriticalRatio >= random;

        // ダメージ
        random = UnityEngine.Random.Range(0.8f, 1f);
        int power = (int)(info.Atk * random);
        bool ignoreDefence = info.IgnoreDefence || isCritical;
        int damage = ignoreDefence ? power : power - defenderStatus.Def;

        // 急所でダメージ倍増
        if (isCritical == true)
            damage = (int)(damage * CR_DAMAGE_UP_RATIO);

        // ダメージは1以上
        damage = Mathf.Max(damage, 1);

        // ヒットしているならHpを削る
        if (isHit == true)
            defenderStatus.RecoverHp(-damage);

        // 死亡しているか
        bool isDead = defenderStatus.Hp == 0;

        return new AttackResult(info.Attacker, defender, isHit, damage, isCritical, defenderStatus.Hp, isDead);
    }

    /// <summary>
    /// 割合ダメージ
    /// </summary>
    /// <param name="info"></param>
    /// <param name="defender"></param>
    /// <returns></returns>
    public static AttackResult DamagePercentage(AttackPercentageInfo info, ICollector defender)
    {
        var defenderStatus = defender.GetInterface<ICharaStatus>().CurrentStatus;

        var random = UnityEngine.Random.Range(0, 1f);
        bool isHit = info.Dex >= random;
        int damage = (int)(defenderStatus.Hp * info.Ratio);
        if (isHit == true)
            defenderStatus.RecoverHp(-damage);

        bool isDead = defenderStatus.Hp == 0;
        return new AttackResult(info.Attacker, defender, isHit, damage, false, defenderStatus.Hp, isDead);
    }

    /// <summary>
    /// 固定ダメージ
    /// </summary>
    /// <param name="info"></param>
    /// <param name="defender"></param>
    /// <returns></returns>
    public static AttackResult DamageFixed(AttackFixedInfo info, ICollector defender)
    {
        var defenderStatus = defender.GetInterface<ICharaStatus>().CurrentStatus;

        var random = UnityEngine.Random.Range(0, 1f);
        bool isHit = info.Dex >= random;
        if (isHit == true)
            defenderStatus.RecoverHp(-info.Damage);

        bool isDead = defenderStatus.Hp == 0;
        return new AttackResult(info.Attacker, defender, isHit, info.Damage, false, defenderStatus.Hp, isDead);
    }
}
