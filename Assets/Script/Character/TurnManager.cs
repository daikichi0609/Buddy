﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using Unity.Collections;
using NaughtyAttributes;
using System.Threading.Tasks;
using static UnityEngine.UI.CanvasScaler;

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
    IDisposable RequestProhibitAction();

    /// <summary>
    /// 誰もアクションしていない
    /// </summary>
    bool NoOneActing { get; }

    /// <summary>
    /// ユニット除去
    /// </summary>
    void RemoveUnit(ICollector unit);
}

public class TurnManager : Singleton<TurnManager, ITurnManager>, ITurnManager
{
    protected override void Awake()
    {
        base.Awake();

        PlayerLoopManager.Interface.GetInitEvent.Subscribe(_ => CreateActionList()).AddTo(this);
        PlayerLoopManager.Interface.GetUpdateEvent.Subscribe(_ => NextUnitAct()).AddTo(this);
    }

    /// <summary>
    /// 行動するキャラ
    /// </summary>
    private List<ICollector> m_ActionUnits = new List<ICollector>();
    [SerializeField, NaughtyAttributes.ReadOnly]
    private int m_ActionIndex;
    void ITurnManager.RemoveUnit(ICollector unit) => m_ActionUnits.Remove(unit);

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
    private Queue<ProhibitRequest> m_ProhibitAllAction = new Queue<ProhibitRequest>();
    [ShowNativeProperty]
    private bool ProhibitAllAction => m_ProhibitAllAction.Count != 0;

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
    /// アクションリスト作成
    /// </summary>
    private void CreateActionList()
    {
        // 階段チェック
        var player = UnitHolder.Interface.Player;
        var checker = player.GetInterface<ICharaCellEventChecker>();
        checker.CheckStairsCell();

        m_ActionUnits.Clear();

        foreach (var friend in UnitHolder.Interface.FriendList)
        {
            friend.GetInterface<ICharaLastActionHolder>().Reset();
            m_ActionUnits.Add(friend);
        }

        foreach (var enemy in UnitHolder.Interface.EnemyList)
        {
            enemy.GetInterface<ICharaLastActionHolder>().Reset();
            m_ActionUnits.Add(enemy);
        }

        // indexリセット
        m_ActionIndex = 0;
        m_ActionUnits[m_ActionIndex].GetInterface<ICharaTurn>().CanBeAct();
    }

    /// <summary>
    /// 次のAiの行動
    /// </summary>
    private void NextUnitAct()
    {
        // 行動禁止中なら何もしない
        if (ProhibitAllAction == true)
        {
            Debug.Log("行動禁止中");
            return;
        }

        // 行動可能なキャラがいるなら何もしない
        foreach (var unit in m_ActionUnits)
        {
            if (unit.GetInterface<ICharaTurn>().CanAct == true)
                return;
        }

        // 行動可能なキャラがいないなら、インクリメントする
        // indexが範囲外なら新しく作る
        if (++m_ActionIndex >= m_ActionUnits.Count)
            CreateActionList();

        // indexが範囲内なら行動させる
        else
        {
            var unit = m_ActionUnits[m_ActionIndex];

            // すでに行動しているなら行動させない
            if (unit.GetInterface<ICharaLastActionHolder>().LastAction != CHARA_ACTION.NONE)
                return;

            unit.GetInterface<ICharaTurn>().CanBeAct();
        }
    }
}

[Serializable]
public readonly struct ProhibitRequest
{
    public ProhibitRequest(GameObject requester)
    {
        Requester = requester;
    }

    private GameObject Requester { get; }
}