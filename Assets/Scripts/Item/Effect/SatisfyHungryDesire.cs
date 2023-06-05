using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(menuName = "MyScriptable/Item/Effect/空腹値回復")]
public class SatisfyHungryDesire : ItemEffectBase
{
    [SerializeField, Header("空腹回復値")]
    private int m_Recover;

    protected override void EffectInternal(ItemEffectContext ctx)
    {
        // Log
        if (ctx.Owner.RequireInterface<ICharaStatus>(out var status) == true)
            ctx.BattleLogManager.Log(status.CurrentStatus.OriginParam.GivenName + "は" + ctx.ItemSetup.ItemName + "を食べた");

        if (ctx.Owner.RequireInterface<ICharaStarvation>(out var starvation) == true)
            starvation.RecoverHungry(m_Recover);
    }
}
