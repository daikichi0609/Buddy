using System;

public class AttackWithPoison : Cleverness
{
    protected override string Name => "ラゴン・ポイズン";
    protected override string Description => "攻撃時、一定確率で相手を毒状態にする。";
    protected override bool CanSwitch => false;

    private static readonly float ADD_POISON_RATIO = 0.3f;

    protected override IDisposable Activate(ClevernessContext ctx)
    {
        var battle = ctx.Owner.GetEvent<ICharaBattleEvent>();
        return battle.RegisterOnPostAttackEvent(async (AttackResult result) =>
        {
            if (result.IsDead == true || result.IsHit == false)
                return;

            if (ProbabilityCalclator.DetectFromPercent(ADD_POISON_RATIO * 100f) == true)
            {
                var status = result.Defender.GetInterface<ICharaCondition>();
                await status.AddCondition(new PoisonCondition(PoisonCondition.POISON_REMAINING_TURN));
            }
        });
    }
}