using System;

public class CriticalHitIfOppConditionPoison : Cleverness
{
    protected override string Name => "ラゴン・ベノム";
    protected override string Description => "毒状態の敵への攻撃が必ずクリティカルヒットする。";
    protected override bool CanSwitch => true;

    protected override IDisposable Activate(ClevernessContext ctx)
    {
        var battleEvent = ctx.Owner.GetEvent<ICharaBattleEvent>();
        return battleEvent.RegisterOnDamageEvent(new BattleSystem.OnDamageEvent(Internal));
    }

    static AttackInfo Internal(AttackInfo info, ICollector defender)
    {
        var abnormal = defender.GetInterface<ICharaStatusAbnormality>();
        if (abnormal.IsPoison == true)
            return new AttackInfo(info.Attacker, info.Name, info.Atk, info.Dex, 1.0f, info.IgnoreDefence, info.Direction);
        return info;
    }
}