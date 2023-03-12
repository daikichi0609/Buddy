using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public interface ITrapHolder : IActorInterface
{
    /// <summary>
    /// 罠取得、あるなら
    /// </summary>
    /// <param name="trap"></param>
    /// <returns></returns>
    bool TryGetTrap(out ITrap trap);

    /// <summary>
    /// 罠設置
    /// </summary>
    /// <param name="trap"></param>
    void SetTrap(ITrap trap);
}

public class TrapHolder : ActorComponentBase, ITrapHolder
{
    /// <summary>
    /// 罠
    /// </summary>
    [SerializeField, ReadOnly, Expandable]
    private ITrap m_Trap;

    /// <summary>
    /// 罠取得
    /// </summary>
    /// <param name="trap"></param>
    /// <returns></returns>
    bool ITrapHolder.TryGetTrap(out ITrap trap)
    {
        trap = m_Trap;
        return trap != null;
    }

    /// <summary>
    /// 罠取得
    /// </summary>
    /// <param name="trap"></param>
    void ITrapHolder.SetTrap(ITrap trap) => m_Trap = trap;

    protected override void Register(ICollector owner)
    {
        base.Register(owner);
        owner.Register<ITrapHolder>(this);
    }
}