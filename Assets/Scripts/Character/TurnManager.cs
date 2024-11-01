﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using Unity.Collections;
using NaughtyAttributes;
using System.Threading.Tasks;
using Zenject;

public interface ITurnManager
{
    /// <summary>
    /// 累計ターン数
    /// </summary>
    int TotalTurnCount { get; }

    /// <summary>
    /// 初めのターン開始時イベント
    /// </summary>
    IObservable<Unit> OnTurnStartFirst { get; }

    /// <summary>
    /// ターン終了イベント
    /// </summary>
    IObservable<int> OnTurnEnd { get; }

    /// <summary>
    /// 次のユニットを行動させる
    /// </summary>
    void NextUnitAct();

    /// <summary>
    /// ユニット行動ストップ
    /// </summary>
    void StopUnitAct();

    /// <summary>
    /// ユニット除去
    /// </summary>
    void RemoveUnit(ICollector unit);

    /// <summary>
    /// アクション禁止
    /// </summary>
    /// <param name="collector"></param>
    /// <returns></returns>
    IDisposable RegisterProhibit(ICollector collector);
}

public class TurnManager : ITurnManager
{
    [Inject]
    private IUnitHolder m_UnitHolder;

    /// <summary>
    /// 行動するキャラリスト
    /// </summary>
    private List<ICollector> m_ActionUnits = new List<ICollector>();
    [SerializeField, NaughtyAttributes.ReadOnly]
    private int m_ActionIndex;

    /// <summary>
    /// フロア移動初回のターン開始時
    /// </summary>
    private Subject<Unit> m_OnTurnStartFirst = new Subject<Unit>();
    IObservable<Unit> ITurnManager.OnTurnStartFirst => m_OnTurnStartFirst;
    private bool m_IsInitialTurn = true;

    /// <summary>
    /// 累計ターン数
    /// </summary>
    [SerializeField, NaughtyAttributes.ReadOnly]
    private ReactiveProperty<int> m_TotalTurnCount = new ReactiveProperty<int>(1);
    IObservable<int> ITurnManager.OnTurnEnd => m_TotalTurnCount.Skip(1);
    int ITurnManager.TotalTurnCount => m_TotalTurnCount.Value;

    /// <summary>
    /// 再帰を止めるフラグ
    /// </summary>
    private bool m_IsStop;

    /// <summary>
    /// アクション禁止リクエスト
    /// </summary>
    private Queue<ProhibitRequest> m_ProhibitRequests = new Queue<ProhibitRequest>();
    private bool ProhibitAllAction => m_ProhibitRequests.Count != 0;

    [Inject]
    public void Construct(IDungeonContentsDeployer dungeonContentsDeployer, IDungeonProgressManager dungeonProgressManager)
    {
        dungeonProgressManager.FloorChanged.SubscribeWithState(this, (_, self) => self.m_IsInitialTurn = true);
        dungeonContentsDeployer.OnDeployContents.SubscribeWithState(this, (_, self) => self.CreateActionList());
    }

    /// <summary>
    /// アクション禁止登録
    /// </summary>
    /// <param name="collector"></param>
    /// <returns></returns>
    IDisposable ITurnManager.RegisterProhibit(ICollector collector)
    {
        m_ProhibitRequests.Enqueue(new ProhibitRequest(collector));
        return Disposable.CreateWithState(m_ProhibitRequests, self => self.Dequeue());
    }

    /// <summary>
    /// 任意のユニットを除く
    /// </summary>
    /// <param name="unit"></param>
    void ITurnManager.RemoveUnit(ICollector unit) => m_ActionUnits.Remove(unit);

    /// <summary>
    /// 再帰停止
    /// </summary>
    void ITurnManager.StopUnitAct()
    {
        m_IsStop = true;
    }

    /// <summary>
    /// 次のAiの行動
    /// </summary>
    private async void NextUnitAct()
    {
        await Task.Delay(1);
        var loop = await NextUnitActInternal();
        if (loop == true)
            NextUnitAct();
    }
    void ITurnManager.NextUnitAct() => NextUnitAct();

    private async Task<bool> NextUnitActInternal()
    {
        // 更新停止
        if (m_IsStop == true)
        {
            m_IsStop = false;
            return false;
        }

        // 行動禁止中
        if (ProhibitAllAction == true)
        {
#if DEBUG
            foreach (var req in m_ProhibitRequests)
                Debug.Log(req.Requester);
#endif
            return true;
        }

        // 最初のターンならイベント実行
        if (m_IsInitialTurn == true)
        {
            m_IsInitialTurn = false;
            m_OnTurnStartFirst.OnNext(Unit.Default);
        }

        // 行動可能なキャラがいないなら、インクリメントする
        // indexが範囲外なら新しくキューを作る
        if (m_ActionIndex >= m_ActionUnits.Count)
            await NextTurn();

        // indexが範囲内なら行動させる
        else
        {
            var unit = m_ActionUnits[m_ActionIndex];
            m_ActionIndex++;

            // 死んでるキャラは行動させない
            var status = unit.GetInterface<ICharaStatus>();
            if (status.CurrentStatus.IsDead == true)
            {
#if DEBUG
                Debug.Log(status.CurrentStatus.OriginParam.GivenName + "は死亡しているので無視します。");
#endif
                return true;
            }

            // すでに行動しているなら行動させない
            if (unit.GetInterface<ICharaLastActionHolder>().LastAction != CHARA_ACTION.NONE)
                return true;

            var condition = unit.GetInterface<ICharaCondition>();
            await condition.FinishCondition();

            // プレイヤー入力
            if (unit.RequireInterface<IPlayerInput>(out var player) == true)
                player.DetectInput();

            // AI行動
            else if (unit.RequireInterface<IAiAction>(out var ai) == true)
                ai.DecideAndExecuteAction();
        }

        return false;
    }

    /// <summary>
    /// 次のターン
    /// </summary>
    private async Task NextTurn()
    {
        m_TotalTurnCount.Value++;
        await CheckStairsCell();
        CreateActionList();
        NextUnitAct();
    }

    /// <summary>
    /// アクションリスト作成
    /// </summary>
    private void CreateActionList()
    {
        // リストクリア
        m_ActionUnits.Clear();

        foreach (var friend in m_UnitHolder.FriendList)
        {
            friend.GetInterface<ICharaLastActionHolder>().Reset();
            m_ActionUnits.Add(friend);
        }

        foreach (var enemy in m_UnitHolder.EnemyList)
        {
            enemy.GetInterface<ICharaLastActionHolder>().Reset();
            m_ActionUnits.Add(enemy);
        }

        // indexリセット
        m_ActionIndex = 0;
    }

    /// <summary>
    /// 階段チェック
    /// </summary>
    private async Task CheckStairsCell()
    {
        // 階段チェック
        var player = m_UnitHolder.Player;
        if (player != null)
        {
            if (player.RequireInterface<ICharaCellEventChecker>(out var checker) == true)
                await checker.CheckStairsCell();
            if (player.RequireInterface<ICharaObjectHolder>(out var holder) == true)
                holder.CharaObject.transform.localPosition = Vector3.zero;
        }
    }
}

[Serializable]
public readonly struct ProhibitRequest
{
    public ProhibitRequest(ICollector requester)
    {
        Requester = requester;
    }

    public ICollector Requester { get; }
}