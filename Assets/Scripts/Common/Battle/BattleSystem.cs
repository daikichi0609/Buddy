using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BattleSystem
{
    public delegate int OnDamageEvent(int damage, AttackInfo info, ICollector defender);

    /// <summary>
    /// ダメージ
    /// </summary>
    /// <param name="info"></param>
    /// <param name="defender"></param>
    /// <returns></returns>
    public static AttackResult Damage(AttackInfo info, ICollector defender)
    {
        var defenderStatus = defender.GetInterface<ICharaStatus>().CurrentStatus;

        // ヒット判定
        float random = UnityEngine.Random.Range(0, 1f);
        bool isHit = info.Dex >= random;

        // ダメージ
        random = UnityEngine.Random.Range(0.8f, 1f);
        int power = (int)(info.Atk * random);
        int damage = info.IgnoreDefence ? power : power - defenderStatus.Def;

        if (info.Attacker.RequireEvent<ICharaBattleEvent>(out var battleEvent) == true && battleEvent.OnDamageEvent != null)
            damage = battleEvent.OnDamageEvent.Invoke(damage, info, defender);

        // 急所判定
        random = UnityEngine.Random.Range(0, 1f);
        bool isCritical = info.CriticalRatio >= random;

        // 急所でダメージ倍増
        if (isCritical == true)
            damage *= 2;

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
