using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;

public interface ITurnManager : ISingleton
{
    int TotalTurnCount { get; }
    IObservable<int> OnTurnEnd { get; }

    bool ProhibitAllAction { get; set; }
    bool NoOneActing { get; }
    void AllCharaActionable();
}

public class TurnManager : Singleton<TurnManager, ITurnManager>, ITurnManager
{
    protected override void Awake()
    {
        base.Awake();

        GameManager.Interface.GetUpdateEvent
            .Subscribe(_ => NextUnitAct()).AddTo(this);
    }

    /// <summary>
    /// 累計ターン数
    /// </summary>
    private ReactiveProperty<int> m_TotalTurnCount = new ReactiveProperty<int>();
    IObservable<int> ITurnManager.OnTurnEnd => m_TotalTurnCount;
    int ITurnManager.TotalTurnCount => m_TotalTurnCount.Value;

    /// <summary>
    /// 全ての行動を禁じる
    /// </summary>
    private bool ProhibitAllAction { get; set; }
    bool ITurnManager.ProhibitAllAction { get => ProhibitAllAction; set => ProhibitAllAction = value; }

    /// <summary>
    /// 全てのキャラが行動中でない
    /// </summary>
    private bool NoOneActing
    {
        get
        {
            foreach(ICollector player in UnitManager.Interface.PlayerList)
                if(player.GetComponent<ICharaTurn>().IsActing == true)
                    return false;

            foreach (ICollector enemy in UnitManager.Interface.EnemyList)
                if (enemy.GetComponent<ICharaTurn>().IsActing == true)
                    return false;

            return true;
        }
    }
    bool ITurnManager.NoOneActing => NoOneActing;

    /// <summary>
    /// 次の行動を促す
    /// </summary>
    private void NextUnitAct()
    {
        if (ProhibitAllAction == true)
            return;

        //プレイヤーの行動待ち
        foreach (ICollector player in UnitManager.Interface.PlayerList)
            if (player.GetComponent<ICharaTurn>().CanAct == true)
                return;

        //敵
        foreach (ICollector enemy in UnitManager.Interface.EnemyList)
            if (enemy.GetComponent<ICharaTurn>().CanAct == true)
            {
                var ai = enemy.GetComponent<IEnemyAi>();
                ai.DecideAndExecuteAction();
                return;
            }

        //全キャラ行動済みなら行動済みステータスをリセット
        AllCharaActionable();
    }

    /// <summary>
    /// 全キャラの行動済みステータスをリセット
    /// </summary>
    private void AllCharaActionable()
    {
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
        foreach (ICollector player in UnitManager.Interface.PlayerList)
            player.GetComponent<ICharaTurn>().CanAct = true;
    }

    /// <summary>
    /// 敵の行動済みステータスをリセット
    /// </summary>
    private void AllEnemyActionable()
    {
        foreach (ICollector enemy in UnitManager.Interface.EnemyList)
            enemy.GetComponent<ICharaTurn>().CanAct = true;
    }
}
