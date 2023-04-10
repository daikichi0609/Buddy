using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using Unity.Collections;
using NaughtyAttributes;
using System.Threading.Tasks;

public interface ITurnManager : ISingleton
{
    /// <summary>
    /// 累計ターン数
    /// </summary>
    int TotalTurnCount { get; }

    /// <summary>
    /// ターン終了イベント
    /// </summary>
    IObservable<int> OnTurnEnd { get; }

    /// <summary>
    /// アクション禁止フラグ
    /// </summary>
    bool ProhibitAllAction { get; }
    IDisposable RequestProhibitAction();

    /// <summary>
    /// 誰もアクションしていない
    /// </summary>
    bool NoOneActing { get; }

    /// <summary>
    /// Ai行動開始
    /// </summary>
    void StartAiAct();

    /// <summary>
    /// 全てのキャラの行動を許可
    /// </summary>
    void AllCharaActionable();
}

public class TurnManager : Singleton<TurnManager, ITurnManager>, ITurnManager
{
    protected override void Awake()
    {
        base.Awake();

        GameManager.Interface.GetInitEvent.Subscribe(_ =>
        {
            AllCharaActionable();
        }).AddTo(this);
    }

    /// <summary>
    /// キャラの行動順
    /// </summary>
    private readonly Queue<ICollector> m_NextActor = new Queue<ICollector>();

    /// <summary>
    /// 更新止めるもの
    /// </summary>
    private IDisposable m_Disposable;

    /// <summary>
    /// 累計ターン数
    /// </summary>
    [SerializeField, NaughtyAttributes.ReadOnly]
    private ReactiveProperty<int> m_TotalTurnCount = new ReactiveProperty<int>(1);
    IObservable<int> ITurnManager.OnTurnEnd => m_TotalTurnCount;
    int ITurnManager.TotalTurnCount => m_TotalTurnCount.Value;

    /// <summary>
    /// 全ての行動を禁じる
    /// </summary>
    private readonly Queue<ProhibitRequest> m_ProhibitAllAction = new Queue<ProhibitRequest>();
    private bool ProhibitAllAction => m_ProhibitAllAction.Count != 0;
    bool ITurnManager.ProhibitAllAction => ProhibitAllAction;

    /// <summary>
    /// 禁止リクエスト
    /// </summary>
    /// <returns></returns>
    IDisposable ITurnManager.RequestProhibitAction()
    {
        m_ProhibitAllAction.Enqueue(new ProhibitRequest());
        return Disposable.Create(() => m_ProhibitAllAction.Dequeue());
    }

    /// <summary>
    /// 全てのキャラが行動中でない
    /// </summary>
    private bool NoOneActing
    {
        get
        {
            foreach (ICollector player in UnitHolder.Interface.FriendList)
                if (player.GetInterface<ICharaTurn>().IsActing == true)
                    return false;

            foreach (ICollector enemy in UnitHolder.Interface.EnemyList)
                if (enemy.GetInterface<ICharaTurn>().IsActing == true)
                    return false;

            return true;
        }
    }
    bool ITurnManager.NoOneActing => NoOneActing;

    /// <summary>
    /// 更新購読
    /// </summary>
    private void SubscribeUpdate()
    {
        m_Disposable = GameManager.Interface.GetUpdateEvent
            .Subscribe(async _ => await NextUnitAct()).AddTo(this);
    }

    /// <summary>
    /// Ai行動開始
    /// </summary>
    private void StartAiAct()
    {
        SubscribeUpdate();
        CreateActionQueue();
    }
    void ITurnManager.StartAiAct() => StartAiAct();

    /// <summary>
    /// キュー作成
    /// </summary>
    private void CreateActionQueue()
    {
        foreach (ICollector player in UnitHolder.Interface.FriendList)
            if (player.RequireInterface<IAiAction>(out var _) == true)
                m_NextActor.Enqueue(player);

        foreach (ICollector enemy in UnitHolder.Interface.EnemyList)
            if (enemy.RequireInterface<IAiAction>(out var _) == true)
                m_NextActor.Enqueue(enemy);
    }

    /// <summary>
    /// 次の行動を促す
    /// </summary>
    private async Task NextUnitAct()
    {
        if (ProhibitAllAction == true)
            return;

        if (m_NextActor.TryPeek(out var unit) == true)
        {
            var turn = unit.GetInterface<ICharaTurn>();
            if (turn.CanAct == false)
            {
                if (unit.RequireInterface<ICharaStatus>(out var status) == true)
                    Debug.Log("行動済のキャラがキューに存在します" + status.Parameter.GivenName);

                m_NextActor.TryDequeue(out var _);
                return;
            }

            // Ai行動
            if (unit.RequireInterface<IAiAction>(out var ai) == true && await ai.DecideAndExecuteAction() == true)
                m_NextActor.TryDequeue(out var _);
            else
                return;
        }
        else
            // 全キャラ行動済みなら行動済みステータスをリセット
            FinishAiAct();
    }

    /// <summary>
    /// Ai行動終了
    /// </summary>
    private void FinishAiAct()
    {
        m_Disposable.Dispose();
        AllCharaActionable();
    }

    /// <summary>
    /// 全キャラの行動済みステータスをリセット
    /// </summary>
    private async void AllCharaActionable()
    {
        // 階段チェック
        var player = UnitHolder.Interface.Player;
        if (player.RequireInterface<ICharaCellEventChecker>(out var checker) == true)
            await checker.CheckStairsCell();

        m_TotalTurnCount.Value++;
        AllPlayerActionable();
        AllEnemyActionable();
    }
    void ITurnManager.AllCharaActionable() => AllCharaActionable();

    /// <summary>
    /// プレイヤーの行動済みステータスをリセット
    /// </summary>
    private void AllPlayerActionable()
    {
        foreach (ICollector player in UnitHolder.Interface.FriendList)
            player.GetInterface<ICharaTurn>().CanBeAct();
    }

    /// <summary>
    /// 敵の行動済みステータスをリセット
    /// </summary>
    private void AllEnemyActionable()
    {
        foreach (ICollector enemy in UnitHolder.Interface.EnemyList)
            enemy.GetInterface<ICharaTurn>().CanBeAct();
    }
}

public readonly struct ProhibitRequest
{

}