using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Apple : ItemEffect, IItemEffect
{
    protected override ITEM_NAME ItemName => ITEM_NAME.APPLE;

    protected override void EffectInternal(ICollector owner)
    {
        // Log
        if (owner.RequireComponent<ICharaStatus>(out var status) == true)
            BattleLogManager.Interface.Log(status.CurrentStatus.Name + "は" + ItemName + "を食べた");

        if (owner.RequireComponent<ICharaStarvation>(out var starvation) == true)
            starvation.RecoverHungry(40);
    }
}
