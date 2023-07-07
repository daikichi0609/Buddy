using System;

public class AttackWithPoison : Cleverness
{
    protected override string Name => "ラゴン・ポイズン";
    protected override string Description => "攻撃時、一定確率で相手を毒状態にする。";
    protected override bool CanSwitch => false;

    private static readonly float BUFF_CR_RATIO = 0.3f;

    protected override IDisposable Activate(ClevernessContext ctx)
    {
        var status = ctx.Owner.GetInterface<ICharaStatus>().CurrentStatus;
        return status.AddBuff(new BuffTicket(PARAMETER_TYPE.CR, BUFF_CR_RATIO));
    }
}