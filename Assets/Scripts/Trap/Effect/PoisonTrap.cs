using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class PoisonTrap : TrapEffectBase
{
    protected override async Task EffectInternal(TrapEffectContext ctx)
    {
        await ctx.EffectHandler.Play(ctx.EffectPos, 0.5f);
        await Task.Delay(100);

        // 毒状態にする
        var condition = ctx.Owner.GetInterface<ICharaCondition>();
        await condition.AddCondition(new PoisonCondition(PoisonCondition.POISON_REMAINING_TURN));
    }
}