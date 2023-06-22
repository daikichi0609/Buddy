using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class RecoverHp : ItemEffectBase
{
    [SerializeField, Header("体力回復値")]
    private int m_Recover;

    protected override Task EffectInternal(ItemEffectContext ctx)
    {
        if (ctx.Owner.RequireInterface<ICharaStatus>(out var status) == true)
            status.CurrentStatus.RecoverHp(m_Recover);

        return Task.CompletedTask;
    }
}
