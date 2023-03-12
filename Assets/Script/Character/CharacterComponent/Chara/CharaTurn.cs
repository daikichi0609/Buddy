using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using System.Threading.Tasks;
using NaughtyAttributes;
using System.Threading;

public interface ICharaTurn : IActorInterface
{
    bool CanAct { get; }
    bool IsActing { get; set; }

    void TurnEnd(bool hasCheck = false);
    void CanBeAct();

    Task WaitFinishActing(Action action);
}

public interface ICharaTurnEvent : IActorEvent
{
    /// <summary>
    /// ターン開始 CanAct -> true
    /// </summary>
    IObservable<bool> OnTurnStart { get; }

    /// <summary>
    /// ターン終了後 CanAct -> false
    /// </summary>
    IObservable<bool> OnTurnEndPost { get; }
}

public class CharaTurn : ActorComponentBase, ICharaTurn, ICharaTurnEvent
{
    private ICharaBattle m_CharaBattle;

    /// <summary>
    /// 行動済みステータス
    /// </summary>
    [SerializeField, ReadOnly]
    private ReactiveProperty<bool> m_CanAct = new ReactiveProperty<bool>(true);
    bool ICharaTurn.CanAct => m_CanAct.Value;

    /// <summary>
    /// CanAct -> true
    /// </summary>
    IObservable<bool> ICharaTurnEvent.OnTurnStart => m_CanAct.Where(turn => turn == true);

    /// <summary>
    /// CanAct -> false
    /// </summary>
    IObservable<bool> ICharaTurnEvent.OnTurnEndPost => m_CanAct.Where(turn => turn == false);

    /// <summary>
    /// 行動中ステータス
    /// </summary>
    [SerializeField, ReadOnly]
    private bool m_IsActing;
    bool ICharaTurn.IsActing
    {
        get => m_IsActing;
        set => m_IsActing = value;
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
        m_CharaBattle = Owner.GetInterface<ICharaBattle>();
    }

    /// <summary>
    /// ターン終了
    /// </summary>
    async void ICharaTurn.TurnEnd(bool hasCheck)
    {
        if (hasCheck == true && Owner.RequireInterface<ICharaCellEventChecker>(out var checker) == true)
            if (await checker.CheckCurrentCell() == true)
                return;

        m_CanAct.Value = false;
        if (Owner.RequireInterface<ICharaTypeHolder>(out var type) == true && type.Type == CHARA_TYPE.PLAYER)
            TurnManager.Interface.StartAiAct();
    }

    /// <summary>
    /// ターン開始
    /// </summary>
    void ICharaTurn.CanBeAct() => m_CanAct.Value = true;

    async Task ICharaTurn.WaitFinishActing(Action action)
    {
        // IsActing -> false になるまで待つ
        while (m_IsActing == true)
            await Task.Delay(1);

        action.Invoke();
    }
}
