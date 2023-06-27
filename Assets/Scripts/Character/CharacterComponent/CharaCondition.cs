using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UniRx;

public interface ICharaCondition : IActorInterface
{
    /// <summary>
    /// 状態異常追加
    /// </summary>
    /// <param name="condition"></param>
    /// <returns></returns>
    Task AddCondition(ICondition condition);

    /// <summary>
    /// 状態異常効果
    /// </summary>
    /// <returns></returns>
    Task EffectCondition();

    /// <summary>
    /// 状態異常終了
    /// </summary>
    /// <returns></returns>
    Task FinishCondition();
}

public class CharaCondition : ActorComponentBase, ICharaCondition
{
    /// <summary>
    /// 現在かかっている状態異常まとめ
    /// </summary>
    private List<ICondition> m_CharaCondition = new List<ICondition>();

    protected override void Register(ICollector owner)
    {
        base.Register(owner);
        owner.Register<ICharaCondition>(this);
    }

    protected override void Initialize()
    {
        base.Initialize();
    }

    /// <summary>
    /// 状態異常追加
    /// </summary>
    /// <param name="condition"></param>
    async Task ICharaCondition.AddCondition(ICondition condition)
    {
        m_CharaCondition.Add(condition);
        await condition.OnStart(Owner);
    }

    /// <summary>
    /// 状態異常効果
    /// </summary>
    /// <returns></returns>
    async Task ICharaCondition.EffectCondition()
    {
        for (int i = 0; i < m_CharaCondition.Count; i++)
        {
            var effect = m_CharaCondition[i];
            await effect.Effect(Owner);
        }
    }

    /// <summary>
    /// 状態異常終了
    /// </summary>
    /// <returns></returns>
    async Task ICharaCondition.FinishCondition()
    {
        for (int i = 0; i < m_CharaCondition.Count; i++)
        {
            var effect = m_CharaCondition[i];
            if (effect.IsFinish == true)
            {
                await effect.OnFinish(Owner);
                m_CharaCondition.Remove(effect);
            }
        }
    }
}
