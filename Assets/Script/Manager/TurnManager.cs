using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;

public interface ITurnManager : ISingleton
{
    bool CanAct { get; }
    void NextUnitAct();
    IEnumerator WaitCanAct(Action action);
    void AllCharaActionable();
}

public class TurnManager : Singleton<TurnManager, ITurnManager>, ITurnManager
{
    /// <summary>
    /// 全キャラに行動を禁止させるフラグ
    /// </summary>
    private bool CanAct
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
    bool ITurnManager.CanAct => CanAct;

    /// <summary>
    /// 次の行動を促す
    /// </summary>
    private void NextUnitAct()
    {
        //プレイヤーの行動待ち
        foreach (ICollector player in UnitManager.Interface.PlayerList)
            if (player.GetComponent<ICharaTurn>().IsFinishTurn == false)
                return;

        //敵
        foreach (ICollector enemy in UnitManager.Interface.EnemyList)
            if (enemy.GetComponent<ICharaTurn>().IsFinishTurn == false)
            {
                var ai = enemy.GetComponent<IEnemyAi>();
                ai.DecideAndExecuteAction();
                return;
            }

        //全キャラ行動済みなら行動済みステータスをリセット
        AllCharaActionable();
    }
    void ITurnManager.NextUnitAct() => NextUnitAct();

    /// <summary>
    /// 攻撃許可が出るまで待ってから実行
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    private IEnumerator WaitCanAct(Action action)
    {
        yield return new WaitUntil(() => CanAct == true);
        action?.Invoke();
    }
    IEnumerator ITurnManager.WaitCanAct(Action action) => WaitCanAct(action);

    /// <summary>
    /// 全キャラの行動済みステータスをリセット
    /// </summary>
    private void AllCharaActionable()
    {
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
            player.GetComponent<ICharaTurn>().StartTurn();
    }

    /// <summary>
    /// 敵の行動済みステータスをリセット
    /// </summary>
    private void AllEnemyActionable()
    {
        foreach (ICollector enemy in UnitManager.Interface.EnemyList)
            enemy.GetComponent<ICharaTurn>().StartTurn();
    }
}
