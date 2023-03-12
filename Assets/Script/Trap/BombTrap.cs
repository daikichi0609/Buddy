using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class BombTrap : TrapBase
{
    protected override async Task EffectInternal(ICollector stepper, IUnitFinder unitFinder)
    {
        return;
    }
}
