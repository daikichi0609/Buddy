using System;

public class AttackUpIfAbnormalCondition : Cleverness
{
    protected override string Name => "逆上";
    protected override string Description => "自分が状態異常にかかっているなら、攻撃力が上昇する";
    protected override bool CanSwitch => true;

    private static readonly int ATTACK_UP_RATIO = 2;

    protected override IDisposable Activate(ClevernessContext ctx)
    {
        var battleEvent = ctx.Owner.GetEvent<ICharaBattleEvent>();
        return battleEvent.RegisterOnDamageEvent(new BattleSystem.OnDamageEvent(Internal));
    }

    static int Internal(int damage, AttackInfo info, ICollector defender)
    {
        var abnormal = info.Attacker.GetInterface<ICharaStatusAbnormality>();
        if (abnormal.IsPoison == true || abnormal.IsSleeping == true)
            damage *= ATTACK_UP_RATIO;
        return damage;
    }
}