using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class CauseFixedDamage : ItemEffectBase
{
    [SerializeField, Header("ダメージ")]
    private int m_Damage;

    protected override async Task EffectInternal(ItemEffectContext ctx)
    {
        var battle = ctx.Owner.GetInterface<ICharaBattle>();
        var status = ctx.Owner.GetInterface<ICharaStatus>();
        battle.Damage(new AttackInfo(ctx.Owner, status.CurrentStatus.OriginParam.GivenName, m_Damage, 100f, 0f, true, DIRECTION.NONE), out var task);
        await task;
    }
}
