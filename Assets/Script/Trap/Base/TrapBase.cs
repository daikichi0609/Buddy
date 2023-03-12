using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public interface ITrap
{
    /// <summary>
    /// 罠タイプ
    /// </summary>
    TRAP_TYPE TrapType { get; }

    /// <summary>
    /// 罠効果
    /// </summary>
    /// <param name="stepper"></param>
    /// <param name="unitFinder"></param>
    /// <returns></returns>
    Task Effect(ICollector stepper, IUnitFinder unitFinder);
}

public class TrapBase : ITrap
{
    protected virtual TRAP_TYPE TrapType { get; } = TRAP_TYPE.NONE;
    TRAP_TYPE ITrap.TrapType => TrapType;

    private static readonly float ACTIVATE_RATE = 0.9f;

    async protected virtual Task EffectInternal(ICollector stepper, IUnitFinder unitFinder)
    {

    }

    async Task ITrap.Effect(ICollector stepper, IUnitFinder unitFinder)
    {
        if (ProbabilityCalclator.DetectFromPercent(ACTIVATE_RATE) == false)
            return;

        await EffectInternal(stepper, unitFinder);
    }
}
