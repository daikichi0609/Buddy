using System;

public class AttackUpIfPoison : Cleverness
{
    protected override string Name => "ラゴン・ベノム";
    protected override string Description => "相手が毒状態なら、与えるダメージが上昇する。";
    protected override bool CanSwitch => true;

    private static readonly int ATTACK_UP_RATIO = 2;

    protected override IDisposable Activate(ClevernessContext ctx)
    {
        var battleEvent = ctx.Owner.GetEvent<ICharaBattleEvent>();
        return battleEvent.RegisterOnDamageEvent(new BattleSystem.OnDamageEvent(Internal));
    }

    static int Internal(int damage, AttackInfo info, ICollector defender)
    {
        var abnormal = defender.GetInterface<ICharaStatusAbnormality>();
        if (abnormal.IsPoison == true)
            damage *= ATTACK_UP_RATIO;
        return damage;
    }
}