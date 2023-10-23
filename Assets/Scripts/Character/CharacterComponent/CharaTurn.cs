using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using System.Threading.Tasks;
using NaughtyAttributes;
using Zenject;

public interface ICharaTurn : IActorInterface
{
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
    /// ターン終了
    /// </summary>
    Task TurnEnd();

    /// <summary>
    /// Acting == false を待つ
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    Task WaitFinishActing();
}

public interface ICharaTurnEvent : IActorEvent
{
    /// <summary>
    /// ターン終了後 CanAct -> false
    /// </summary>
    IObservable<Unit> OnTurnEnd { get; }
}

public class CharaTurn : ActorComponentBase, ICharaTurn, ICharaTurnEvent
{
    [Inject]
    private IMiniMapRenderer m_MiniMapRenderer;
    [Inject]
    private ITurnManager m_TurnManager;

    private ICharaLastActionHolder m_CharaLastCharaActionHolder;
    private ICharaCondition m_CharaCondition;

    /// <summary>
    /// ターン終了時
    /// </summary>
    private Subject<Unit> m_OnTurnEnd = new Subject<Unit>();
    IObservable<Unit> ICharaTurnEvent.OnTurnEnd => m_OnTurnEnd;

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
        m_CharaLastCharaActionHolder = Owner.GetInterface<ICharaLastActionHolder>();
        m_CharaCondition = Owner.GetInterface<ICharaCondition>();

        // ミニマップアイコン登録
        var disposable = m_MiniMapRenderer.RegisterIcon(Owner);
        Owner.Disposables.Add(disposable);
        // ターン終了時にアイコン更新
        m_OnTurnEnd.SubscribeWithState(this, (_, self) => self.m_MiniMapRenderer.ReflectIcon(self.Owner)).AddTo(Owner.Disposables);
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
    async Task ICharaTurn.TurnEnd()
    {
        // 行動禁止
        var disposable = m_TurnManager.RegisterProhibit(Owner);

        var checker = Owner.GetInterface<ICharaCellEventChecker>();
        var lastAction = m_CharaLastCharaActionHolder.LastAction;
        if (lastAction == CHARA_ACTION.NONE)
            Debug.LogWarning("アクションしていないのにターン終了しようとしています");

        bool check = lastAction switch
        {
            CHARA_ACTION.MOVE => true,
            _ => false
        };

        // 移動したなら現在地セルのイベント確認
        if (check == true)
            await checker.CheckCurrentCell();

        // ターン終了イベント
        m_OnTurnEnd.OnNext(Unit.Default);

        // 状態異常・バフ系の更新
        await m_CharaCondition.EffectCondition();

        // 行動禁止解除
        disposable.Dispose();

        // 入れ替わり後処理
        var switchInfo = Owner.GetInterface<ICharaMove>().SwitchInfo;
        if (switchInfo != null)
        {
            await CheckSwitcher(switchInfo);
            return;
        }

        // 次のUnit行動
        m_TurnManager.NextUnitAct();
    }

    private async Task CheckSwitcher(CharaMove.MoveSwitchInfo info)
    {
        var turn = info.Switcher.GetInterface<ICharaTurn>();
        await turn.TurnEnd();

        Owner.GetInterface<ICharaMove>().SwitchInfo = null;
    }

    /// <summary>
    /// IsActing == false になるのを待ってから発火
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="arg"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    async Task ICharaTurn.WaitFinishActing()
    {
        // IsActing -> false になるまで待つ
        while (IsActing == true)
            await Task.Delay(1);
    }

    [Serializable]
    private struct ActTicket { }
}
