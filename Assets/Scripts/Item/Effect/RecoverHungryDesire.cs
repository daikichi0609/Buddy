using System.Threading.Tasks;
using UnityEngine;

public class RecoverHungryDesire : ItemEffectBase
{
    protected override Task EffectInternal(ItemEffectContext ctx)
    {
        if (ctx.Owner.RequireInterface<ICharaStatus>(out var status) == true)
            ctx.BattleLogManager.Log(status.CurrentStatus.OriginParam.GivenName + "のお腹が膨れた");

        return Task.CompletedTask;
    }
}
