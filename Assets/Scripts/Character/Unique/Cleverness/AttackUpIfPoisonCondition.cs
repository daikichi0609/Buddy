using System;
using UniRx;

public class AttackUpIfPoisonCondition : Cleverness
{
    protected override string Name => "逆上";
    protected override string Description => "自分が毒状態なら、攻撃力が上昇する";
    protected override bool CanSwitch => true;

    protected override IDisposable Activate(ClevernessContext ctx)
    {
        var abnormal = ctx.Owner.GetInterface<ICharaStatusAbnormality>();
        abnormal.CanFrenzy = true;
        return Disposable.CreateWithState(abnormal, abnormal => abnormal.CanFrenzy = false);
    }
}