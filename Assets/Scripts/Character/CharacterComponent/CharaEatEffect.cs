using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;
using Zenject;
using static UnityEngine.UI.GridLayoutGroup;

public interface ICharaEatEffect : IActorInterface
{
    /// <summary>
    /// 特別な食事効果
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    Task EatEffect(ItemSetup item);

    /// <summary>
    /// 攻撃力アップ効果登録
    /// </summary>
    /// <returns></returns>
    IDisposable SetFoodAttackUp(ItemSetup item);

    /// <summary>
    /// クリティカル率アップ効果
    /// </summary>
    /// <returns></returns>
    IDisposable SetFoodCriticalRatioUp(ItemSetup item);
}

public class CharaEatEffect : ActorComponentBase, ICharaEatEffect
{
    private ICharaCondition m_CharaCondition;

    /// <summary>
    /// 攻撃力アップアイテム
    /// </summary>
    private ItemSetup m_AttackUpFood;

    /// <summary>
    /// クリティカル率アップアイテム
    /// </summary>
    private ItemSetup m_CriticalRatioUpFood;

    private static readonly int REMAINING_TURN = 3;

    protected override void Register(ICollector owner)
    {
        base.Register(owner);
        owner.Register(this);
    }

    protected override void Initialize()
    {
        base.Initialize();
        m_CharaCondition = Owner.GetInterface<ICharaCondition>();
    }

    /// <summary>
    /// 特別な食事効果
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    async Task ICharaEatEffect.EatEffect(ItemSetup item)
    {
        if (item == m_AttackUpFood)
            await m_CharaCondition.AddCondition(new FoodAttackUpCondition(REMAINING_TURN));

        if (item == m_CriticalRatioUpFood)
            await m_CharaCondition.AddCondition(new FoodCriticalRatioUpCondition(REMAINING_TURN));
    }

    /// <summary>
    /// 攻撃力アップ効果登録
    /// </summary>
    /// <param name="item"></param>
    /// <param name="mag"></param>
    /// <returns></returns>
    IDisposable ICharaEatEffect.SetFoodAttackUp(ItemSetup item)
    {
        if (m_AttackUpFood != null)
            return null;

        m_AttackUpFood = item;

        return Disposable.CreateWithState(this, self =>
        {
            self.m_AttackUpFood = null;
        });
    }

    /// <summary>
    /// クリティカル率アップ登録
    /// </summary>
    /// <param name="item"></param>
    /// <param name="mag"></param>
    /// <returns></returns>
    IDisposable ICharaEatEffect.SetFoodCriticalRatioUp(ItemSetup item)
    {
        if (m_CriticalRatioUpFood != null)
            return null;

        m_CriticalRatioUpFood = item;

        return Disposable.CreateWithState(this, self =>
        {
            self.m_CriticalRatioUpFood = null;
        });
    }

    protected override void Dispose()
    {
        // m_AttackUpFood = null;
        // m_CriticalRatioUpFood = null;
        base.Dispose();
    }
}
