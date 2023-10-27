using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UniRx;
using Zenject;

public interface ICharaCondition : IActorInterface
{
    /// <summary>
    /// 状態異常追加
    /// </summary>
    /// <param name="condition"></param>
    /// <returns></returns>
    Task<bool> AddCondition(ICondition condition);

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
    [Inject]
    private IEffectHolder m_EffectHolder;
    [Inject]
    private ISoundHolder m_SoundHolder;

    /// <summary>
    /// 現在かかっている状態異常まとめ
    /// </summary>
    private List<ICondition> m_CharaCondition = new List<ICondition>();

    protected override void Register(ICollector owner)
    {
        base.Register(owner);
        owner.Register<ICharaCondition>(this);
    }

    protected override async void Dispose()
    {
        foreach (var condition in m_CharaCondition)
            await condition.OnFinish(Owner);
        m_CharaCondition.Clear();
        base.Dispose();
    }

    /// <summary>
    /// 状態異常追加
    /// </summary>
    /// <param name="condition"></param>
    async Task<bool> ICharaCondition.AddCondition(ICondition condition)
    {
        condition.Register(m_EffectHolder, m_SoundHolder);
        var success = await condition.OnStart(Owner, m_EffectHolder, m_SoundHolder);

        if (success == true)
        {
            // 重複不可な状態異常を解除
            if (condition.CanOverlapping == false)
                for (int i = 0; i < m_CharaCondition.Count; i++)
                {
                    var c = m_CharaCondition[i];
                    if (c.CanOverlapping == false)
                    {
                        await c.OnFinish(Owner);
                        m_CharaCondition.Remove(c);
                    }
                }

            // 状態異常付与
            m_CharaCondition.Add(condition);
        }
        return success;
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
    private async Task FinishCondition()
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
    Task ICharaCondition.FinishCondition() => FinishCondition();
}
