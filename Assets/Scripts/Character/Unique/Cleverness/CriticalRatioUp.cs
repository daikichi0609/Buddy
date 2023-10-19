using System;

public class CriticalRatioUp : Cleverness
{
    protected override string Name => "ふみだすゆうき";
    protected override string Description => "クリティカル率が上昇する。";
    protected override bool CanSwitch => true;

    private static readonly float BUFF_CR_RATIO = 2f;

    protected override IDisposable Activate(ClevernessContext ctx)
    {
        var status = ctx.Owner.GetInterface<ICharaStatus>().CurrentStatus;
        return status.AddBuff(new BuffTicket(PARAMETER_TYPE.CR, BUFF_CR_RATIO));
    }
}