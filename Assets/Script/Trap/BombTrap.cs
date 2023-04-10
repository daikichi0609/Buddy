using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class BombTrap : TrapBase
{
    protected override TRAP_TYPE TrapType => TRAP_TYPE.BOMB;

    protected override async Task EffectInternal(ICollector stepper, IUnitFinder unitFinder, AroundCell aroundCell)
    {
        return;
    }
}
