using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;

public interface ICharaTurn : ICharacterComponent
{
    bool CanAct { get; set; }
    bool IsActing { get; set; }
}

public interface ICharaTurnEvent : ICharacterComponent
{
    IObservable<bool> OnTurnStart { get; }
    IObservable<bool> OnTurnEnd { get; }
}

public class CharaTurn : CharaComponentBase, ICharaTurn, ICharaTurnEvent
{
    /// <summary>
    /// 行動済みステータス
    /// </summary>
    private ReactiveProperty<bool> m_CanAct = new ReactiveProperty<bool>();
    bool ICharaTurn.CanAct
    {
        get => m_CanAct.Value;
        set => m_CanAct.Value = value;
    }
    IObservable<bool> ICharaTurnEvent.OnTurnStart => m_CanAct.Where(turn => turn == true);
    IObservable<bool> ICharaTurnEvent.OnTurnEnd => m_CanAct.Where(turn => turn == false);

    /// <summary>
    /// 行動中ステータス
    /// </summary>
    private bool IsActing { get; set; } = false;
    bool ICharaTurn.IsActing
    {
        get => IsActing;
        set => IsActing = value;
    }

    protected override void Register(ICollector owner)
    {
        base.Register(owner);
        owner.Register<ICharaTurn>(this);
        owner.Register<ICharaTurnEvent>(this);
    }

    protected override void Initialize()
    {
        base.Initialize();

        if (Owner.RequireComponent<ICharaBattleEvent>(out var battle) == true)
        {
            // 攻撃
            battle.OnAttackStart.Subscribe(_ => m_CanAct.Value = false).AddTo(this);
        }

        if (Owner.RequireComponent<ICharaMoveEvent>(out var move) == true)
        {
            // 移動前
            move.OnMoveStart.Subscribe(_ => m_CanAct.Value = false).AddTo(this);
        }
    }
}
