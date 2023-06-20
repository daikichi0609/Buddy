using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using NaughtyAttributes;
using Zenject;

public interface ICellStateChanger : IActorInterface
{
    /// <summary>
    /// 探索済みかどうか
    /// </summary>
    bool IsExplored { set; }

    /// <summary>
    /// 見えている罠があるか
    /// </summary>
    bool IsVisibleTrap { set; }
}

public interface ICellStateChangeEvent : IActorEvent
{
    /// <summary>
    /// 探索済みステータス変更時
    /// </summary>
    IObservable<bool> IsExploredChanged { get; }

    /// <summary>
    /// トラップ可視ステータス変更時
    /// </summary>
    IObservable<bool> IsVisibleTrapChanged { get; }
}

public class CellStateHandler : ActorComponentBase, ICellStateChanger, ICellStateChangeEvent
{
    [Inject]
    private IMiniMapRenderer m_MiniMapRenderer;

    /// <summary>
    /// 探索済みかどうか
    /// </summary>
    private ReactiveProperty<bool> m_IsExplored = new ReactiveProperty<bool>();
    IObservable<bool> ICellStateChangeEvent.IsExploredChanged => m_IsExplored;
    [ShowNativeProperty]
    private bool IsExplored => m_IsExplored.Value;
    bool ICellStateChanger.IsExplored { set => m_IsExplored.Value = value; }

    /// <summary>
    /// 罠が見えているかどうか
    /// </summary>
    private ReactiveProperty<bool> m_IsVisibleTrap = new ReactiveProperty<bool>();
    IObservable<bool> ICellStateChangeEvent.IsVisibleTrapChanged => m_IsVisibleTrap;
    [ShowNativeProperty]
    private bool IsVisibleTrap => m_IsVisibleTrap.Value;
    bool ICellStateChanger.IsVisibleTrap { set => m_IsVisibleTrap.Value = value; }

    protected override void Register(ICollector owner)
    {
        base.Register(owner);
        owner.Register<ICellStateChanger>(this);
        owner.Register<ICellStateChangeEvent>(this);
    }
}
