using System;

public class AttackUpIfPoison : Cleverness
{
    protected override string Name => "ラゴン・ベノム";
    protected override string Description => "相手が毒状態なら、与えるダメージが上昇する。";
    protected override bool CanSwitch => false;

    private static readonly float BUFF_CR_RATIO = 2.0f;

    protected override IDisposable Activate(ClevernessContext ctx)
    {
        var status = ctx.Owner.GetInterface<ICharaStatus>().CurrentStatus;
        return status.AddBuff(new BuffTicket(PARAMETER_TYPE.CR, BUFF_CR_RATIO));
    }
}