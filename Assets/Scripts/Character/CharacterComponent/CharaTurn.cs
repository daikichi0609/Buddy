using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using System.Threading.Tasks;
using NaughtyAttributes;
using System.Threading;
using ModestTree.Util;

public interface ICharaTurn : IActorInterface
{
    /// <summary>
    /// 行動可能か
    /// </summary>
    bool CanAct { get; }

    /// <summary>
    /// 行動中か
    /// </summary>
    bool IsActing { get; }

    /// <summary>
    /// 行動開始
    /// </summary>
    /// <returns></returns>
    IDisposable RegisterActing();

    /// <summary>
    /// ターン開始
    /// </summary>
    void CanBeAct();

    /// <summary>
    /// ターン終了
    /// </summary>
    void TurnEnd();

    /// <summary>
    /// Acting == false を待って発火
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    Task WaitFinishActing<T>(T self, Action<T> func);
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
    private ICharaLastActionHolder m_CharaLastCharaActionHolder;

    /// <summary>
    /// 行動済みステータス
    /// </summary>
    [SerializeField, ReadOnly]
    private ReactiveProperty<bool> m_CanAct = new ReactiveProperty<bool>(false);
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
    private Queue<ActTicket> m_TicketHolder = new Queue<ActTicket>();
    [ShowNativeProperty]
    private bool IsActing => m_TicketHolder.Count != 0;
    bool ICharaTurn.IsActing => IsActing;

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
        m_CharaLastCharaActionHolder = Owner.GetInterface<ICharaLastActionHolder>();
    }

    /// <summary>
    /// 行動登録
    /// </summary>
    /// <returns></returns>
    IDisposable ICharaTurn.RegisterActing()
    {
        var ticket = new ActTicket();
        m_TicketHolder.Enqueue(ticket);
        return Disposable.CreateWithState(this, self => self.m_TicketHolder.Dequeue());
    }

    /// <summary>
    /// ターン終了
    /// </summary>
    void ICharaTurn.TurnEnd()
    {
        if (Owner.RequireInterface<ICharaCellEventChecker>(out var checker) == true)
        {
            var lastAction = m_CharaLastCharaActionHolder.LastAction;
            if (lastAction == CHARA_ACTION.NONE)
                Debug.LogAssertion("アクションしていないのにターン終了しようとしています");

            bool check = lastAction switch
            {
                CHARA_ACTION.MOVE => true,
                _ => false
            };

            if (check == true)
                checker.CheckCurrentCell();
        }

        m_CanAct.Value = false;
    }

    /// <summary>
    /// ターン開始
    /// </summary>
    void ICharaTurn.CanBeAct()
    {
        m_CanAct.Value = true;
    }

    async Task ICharaTurn.WaitFinishActing<T>(T self, Action<T> action)
    {
        // IsActing -> false になるまで待つ
        while (IsActing == true)
            await Task.Delay(1);

        action.Invoke(self);
    }

    [Serializable]
    private struct ActTicket { }
}
