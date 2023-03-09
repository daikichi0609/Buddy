using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITrap
{
    void Effect(ICollector stepper, IUnitFinder unitFinder);
}

public class TrapBase : ITrap
{
    protected virtual void Effect(ICollector stepper, IUnitFinder unitFinder)
    {

    }

    void ITrap.Effect(ICollector stepper, IUnitFinder unitFinder) => Effect(stepper, unitFinder);
}
