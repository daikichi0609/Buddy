using System.Threading.Tasks;
using UnityEngine;

public class BePoison : ItemEffectBase
{
    protected override async Task EffectInternal(ItemEffectContext ctx)
    {
        if (ctx.Owner.RequireInterface<ICharaCondition>(out var condition) == true)
            await condition.AddCondition(new PoisonCondition(PoisonCondition.POISON_REMAINING_TURN));
    }
}
