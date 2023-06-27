using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class SleepTrap : TrapEffectBase
{

    protected override async Task EffectInternal(TrapEffectContext ctx)
    {
        await ctx.EffectHandler.Play(ctx.EffectPos);

        // 眠り状態にする
        var condition = ctx.Owner.GetInterface<ICharaCondition>();
        await condition.AddCondition(new SleepCondition(Random.Range(2, 5)));
    }
}
