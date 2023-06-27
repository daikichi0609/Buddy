using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class FallAsleep : ItemEffectBase
{
    protected override async Task EffectInternal(ItemEffectContext ctx)
    {
        if (ctx.Owner.RequireInterface<ICharaCondition>(out var condition) == true)
            await condition.AddCondition(new SleepCondition(Random.Range(2, 5)));
    }
}
