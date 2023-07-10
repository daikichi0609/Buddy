using System;

public class AttackUp : Cleverness
{
    protected override string Name => "ブレイバー・ラゴン";
    protected override string Description => "攻撃力が上昇する。";
    protected override bool CanSwitch => false;

    private static readonly float BUFF_ATTACK = 1.5f;

    protected override IDisposable Activate(ClevernessContext ctx)
    {
        var status = ctx.Owner.GetInterface<ICharaStatus>().CurrentStatus;
        return status.AddBuff(new BuffTicket(PARAMETER_TYPE.CR, BUFF_ATTACK));
    }
}