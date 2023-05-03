using System.Collections;
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
    /// 誰もアクションしていない
    /// </summary>
    bool NoOneActing { get; }

    /// <summary>
    /// アクション禁止フラグ
    /// </summary>
    IDisposable RequestProhibitAction(ICollector requester);

    /// <summary>
    /// アクションリスト作成
    /// </summary>
    void CreateActionList();

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

        PlayerLoopManager.Interface.GetUpdateEvent.Subscribe(_ => NextUnitAct()).AddTo(this);
    }

    /// <summary>
    /// 行動するキャラ
    /// </summary>
    private List<ICollector> m_ActionUnits = new List<ICollector>();
    [SerializeField, NaughtyAttributes.ReadOnly]
    private int m_ActionIndex;

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
    IDisposable ITurnManager.RequestProhibitAction(ICollector requester)
    {
        m_ProhibitAllAction.Enqueue(new ProhibitRequest(requester));
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
    /// 階段チェック
    /// </summary>
    private void CheckStairsCell()
    {
        // 階段チェック
        var player = UnitHolder.Interface.Player;
        var checker = player.GetInterface<ICharaCellEventChecker>();
        checker.CheckStairsCell();
    }

    /// <summary>
    /// アクションリスト作成
    /// </summary>
    private void CreateActionList()
    {
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
    void ITurnManager.CreateActionList() => CreateActionList();
    void ITurnManager.RemoveUnit(ICollector unit) => m_ActionUnits.Remove(unit);

    /// <summary>
    /// 次のAiの行動
    /// </summary>
    private void NextUnitAct()
    {
        // 行動禁止中なら何もしない
        if (ProhibitAllAction == true)
        {
#if DEBUG
            foreach (var req in m_ProhibitAllAction)
            {
                if (req.Requester == null)
                    continue;

                if (req.Requester.RequireInterface<ICharaStatus>(out var status) == true)
                {
                    Debug.Log(status.CurrentStatus.Name + "は行動禁止要請中");
                }
                else
                {
                    Debug.Log("差出人不明な行動禁止");
                }
            }
#endif
            return;
        }

        // 行動可能なキャラがいるなら何もしない
        foreach (var unit in m_ActionUnits)
        {
            var status = unit.GetInterface<ICharaStatus>();
            // 死んでるキャラは除外する
            if (status.IsDead == true)
            {
#if DEBUG
                Debug.Log(status.CurrentStatus.Name + "は死亡しているのでアクションリストから除外しました");
#endif
                m_ActionUnits.Remove(unit);
                return;
            }

            // 行動可能なキャラの行動を待つ
            if (unit.GetInterface<ICharaTurn>().CanAct == true)
                return;
        }

        // 行動可能なキャラがいないなら、インクリメントする
        // indexが範囲外なら新しくキューを作る
        if (++m_ActionIndex >= m_ActionUnits.Count)
        {
            CheckStairsCell();
            CreateActionList();
        }
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
    public ProhibitRequest(ICollector requester)
    {
        Requester = requester;
    }

    public ICollector Requester { get; }
}