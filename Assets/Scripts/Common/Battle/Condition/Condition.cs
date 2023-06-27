using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System.Threading.Tasks;
using System;

public interface ICondition
{
    /// <summary>
    /// 開始時
    /// </summary>
    /// <param name="owner"></param>
    /// <returns></returns>
    Task OnStart(ICollector owner);

    /// <summary>
    /// 効果
    /// </summary>
    /// <param name="owner"></param>
    /// <returns></returns>
    Task Effect(ICollector owner);

    /// <summary>
    /// 終了時
    /// </summary>
    /// <param name="owner"></param>
    /// <returns></returns>
    Task OnFinish(ICollector owner);

    /// <summary>
    /// 
    /// </summary>
    bool IsFinish { get; }
}

public abstract class Condition : ICondition
{
    public Condition(int remainingTurn) => m_RemainingTurn = remainingTurn;

    /// <summary>
    /// 残り継続ターン
    /// </summary>
    private int m_RemainingTurn;
    bool ICondition.IsFinish => m_RemainingTurn == 0;

    /// <summary>
    /// 終了時
    /// </summary>
    protected CompositeDisposable m_OnFinish = new CompositeDisposable();

    /// <summary>
    /// 状態異常効果
    /// </summary>
    /// <param name="owner"></param>
    /// <returns></returns>
    async Task ICondition.Effect(ICollector owner)
    {
        if (m_RemainingTurn > 0)
        {
            m_RemainingTurn--;
            await EffectInternal(owner);
        }
    }

    protected abstract Task OnStart(ICollector owner);
    async Task ICondition.OnStart(ICollector owner) => await OnStart(owner);

    protected abstract Task EffectInternal(ICollector owner);

    protected abstract Task OnFinish(ICollector owner);
    async Task ICondition.OnFinish(ICollector owner) => await OnFinish(owner);
}
