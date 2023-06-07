using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BattleSystem
{
    public static AttackResult Damage(AttackInfo info, ICollector defender)
    {
        var defenderStatus = defender.GetInterface<ICharaStatus>().CurrentStatus;

        // ヒット判定
        float random = UnityEngine.Random.Range(0, 1f);
        bool isHit = info.Dex >= random;

        // ダメージ
        int damage = info.IgnoreDefence ? info.Atk : info.Atk - defenderStatus.Def;

        // 急所判定
        random = UnityEngine.Random.Range(0, 1f);
        bool isCritical = info.CriticalRatio >= random;

        // 急所でダメージ倍増
        if (isCritical == true)
            damage *= 2;

        int hp = Mathf.Clamp(defenderStatus.Hp - damage, 0, int.MaxValue);
        bool isDead = hp == 0;

        defenderStatus.Hp = hp;
        return new AttackResult(defender, isHit, damage, isCritical, hp, isDead);
    }

    public static AttackResult DamagePercentage(AttackPercentageInfo info, ICollector defender)
    {
        var defenderStatus = defender.GetInterface<ICharaStatus>().CurrentStatus;

        var random = UnityEngine.Random.Range(0, 1f);
        bool isHit = info.Dex >= random;
        int damage = (int)(defenderStatus.Hp * info.Ratio);
        int hp = Mathf.Clamp(defenderStatus.Hp - damage, 0, int.MaxValue);
        bool isDead = hp == 0;

        defenderStatus.Hp = hp;
        return new AttackResult(defender, isHit, damage, false, hp, isDead);
    }
}
