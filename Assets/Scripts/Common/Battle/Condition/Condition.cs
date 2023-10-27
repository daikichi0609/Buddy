using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System.Threading.Tasks;
using System;

public interface ICondition
{
    /// <summary>
    /// エフェクト登録
    /// </summary>
    /// <param name="effectHolder"></param>
    /// <param name="soundHolder"></param>
    void Register(IEffectHolder effectHolder, ISoundHolder soundHolder);

    /// <summary>
    /// 開始時
    /// </summary>
    /// <param name="owner"></param>
    /// <returns></returns>
    Task<bool> OnStart(ICollector owner, IEffectHolder effectHolder, ISoundHolder soundHolder);

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
    /// 終了しているかどうか
    /// </summary>
    bool IsFinish { get; }

    /// <summary>
    /// 他と共存できるか
    /// </summary>
    bool CanOverlapping { get; }
}

public abstract class Condition : ICondition
{
    public Condition(int remainingTurn)
    {
        m_RemainingTurn = remainingTurn;
    }

    /// <summary>
    /// 残り継続ターン
    /// </summary>
    protected int m_RemainingTurn;
    bool ICondition.IsFinish => m_RemainingTurn == 0;

    /// <summary>
    /// エフェクト
    /// </summary>
    protected IEffectHandler m_EffectHandler = new EffectHandler();

    /// <summary>
    /// 他バフとの共存が可能か
    /// </summary>
    protected abstract bool CanOverlapping { get; }
    bool ICondition.CanOverlapping => CanOverlapping;

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

    /// <summary>
    /// エフェクト登録
    /// </summary>
    /// <param name="effectHolder"></param>
    /// <param name="soundHolder"></param>
    protected abstract void Register(IEffectHolder effectHolder, ISoundHolder soundHolder);
    void ICondition.Register(IEffectHolder effectHolder, ISoundHolder soundHolder) => Register(effectHolder, soundHolder);

    /// <summary>
    /// Condition付与時
    /// </summary>
    /// <param name="owner"></param>
    /// <returns></returns>
    protected abstract Task<bool> OnStart(ICollector owner, IEffectHolder effectHolder, ISoundHolder soundHolder);
    async Task<bool> ICondition.OnStart(ICollector owner, IEffectHolder effectHolder, ISoundHolder soundHolder) => await OnStart(owner, effectHolder, soundHolder);

    /// <summary>
    /// Condition効果
    /// </summary>
    /// <param name="owner"></param>
    /// <returns></returns>
    protected abstract Task EffectInternal(ICollector owner);

    /// <summary>
    /// Condition終了時
    /// </summary>
    /// <param name="owner"></param>
    /// <returns></returns>
    protected abstract Task OnFinish(ICollector owner);
    async Task ICondition.OnFinish(ICollector owner) => await OnFinish(owner);
}
