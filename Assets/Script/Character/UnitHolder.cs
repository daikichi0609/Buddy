using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using System.Linq;
using NaughtyAttributes;

public interface IUnitHolder : ISingleton
{
    /// <summary>
    /// プレイヤー
    /// </summary>
    List<ICollector> PlayerList { get; }

    /// <summary>
    /// 敵
    /// </summary>
    List<ICollector> EnemyList { get; }

    /// <summary>
    /// プレイヤー追加
    /// </summary>
    /// <param name="player"></param>
    void AddPlayer(ICollector player);

    /// <summary>
    /// 敵追加
    /// </summary>
    /// <param name="enemy"></param>
    void AddEnemy(ICollector enemy);

    /// <summary>
    /// ユニット削除
    /// </summary>
    /// <param name="unit"></param>
    void RemoveUnit(ICollector unit);
}

public class UnitHolder : Singleton<UnitHolder, IUnitHolder>, IUnitHolder
{
    /// <summary>
    /// ReactiveCollection
    /// 
    /// 要素の追加
    /// 要素の削除
    /// 要素数の変化
    /// 要素の上書き
    /// 要素の移動
    /// リストのクリア
    /// </summary>

    /// <summary>
    /// Playerのコレクターリスト
    /// </summary>
    [ReadOnly]
    private List<ICollector> m_PlayerList = new List<ICollector>();
    List<ICollector> IUnitHolder.PlayerList => m_PlayerList;

    /// <summary>
    /// Enemyのコレクターリスト
    /// </summary>
    [ReadOnly]
    private List<ICollector> m_EnemyList = new List<ICollector>();
    List<ICollector> IUnitHolder.EnemyList => m_EnemyList;

    void IUnitHolder.AddPlayer(ICollector player) => m_PlayerList.Add(player);
    void IUnitHolder.AddEnemy(ICollector enemy) => m_EnemyList.Add(enemy);
    void IUnitHolder.RemoveUnit(ICollector unit)
    {
        foreach (ICollector player in m_PlayerList)
        {
            if (player == unit)
            {
                m_PlayerList.Remove(unit);
                return;
            }
        }

        foreach (ICollector enemy in m_EnemyList)
        {
            if (enemy == unit)
            {
                m_EnemyList.Remove(unit);
                return;
            }
        }
    }
}
