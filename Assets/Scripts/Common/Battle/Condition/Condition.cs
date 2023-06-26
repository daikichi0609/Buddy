using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System.Threading.Tasks;

public interface ICondition
{
    Task<bool> Effect(ICollector owner);
}

public abstract class Condition : ICondition
{
    public Condition(int remainingTurn) => m_RemainingTurn = remainingTurn;

    /// <summary>
    /// 残り継続ターン
    /// </summary>
    private int m_RemainingTurn;

    /// <summary>
    /// 状態異常効果
    /// </summary>
    /// <param name="owner"></param>
    /// <returns></returns>
    async Task<bool> ICondition.Effect(ICollector owner)
    {
        if (m_RemainingTurn > 0)
        {
            m_RemainingTurn--;
            await EffectInternal(owner);
        }
        return m_RemainingTurn == 0;
    }

    protected abstract Task EffectInternal(ICollector owner);
}
