using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(menuName = "MyScriptable/Item/Effect/空腹値回復")]
public class SatisfyHungryDesire : ItemEffectBase
{
    [SerializeField, Header("空腹回復値")]
    private int m_Recover;

    protected override void EffectInternal(ICollector owner, IItemHandler item)
    {
        // Log
        if (owner.RequireInterface<ICharaStatus>(out var status) == true)
            BattleLogManager.Interface.Log(status.CurrentStatus.OriginParam.GivenName + "は" + item.Setup.ItemName + "を食べた");

        if (owner.RequireInterface<ICharaStarvation>(out var starvation) == true)
            starvation.RecoverHungry(m_Recover);
    }
}
