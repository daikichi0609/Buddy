using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UniRx;

public interface ICharaCondition
{
    void AddCondition(ICondition condition);
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

        if (Owner.RequireEvent<ICharaTurnEvent>(out var turn) == true)
            turn.OnTurnEndPost.SubscribeWithState(this, async (_, self) => await self.EffectCondition()).AddTo(Owner.Disposables);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="condition"></param>
    void ICharaCondition.AddCondition(ICondition condition)
    {
        m_CharaCondition.Add(condition);
    }

    private async Task EffectCondition()
    {
        for (int i = 0; i < m_CharaCondition.Count; i++)
        {
            var effect = m_CharaCondition[i];
            var finish = await effect.Effect(Owner);
            if (finish == true)
                m_CharaCondition.Remove(effect);
        }
    }
}
