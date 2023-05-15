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
    ICollector Player { get; }

    /// <summary>
    /// 味方
    /// </summary>
    List<ICollector> FriendList { get; }

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

    protected override void Awake()
    {
        base.Awake();
        DungeonContentsDeployer.Interface.OnRemoveContents.Subscribe(_ =>
        {
            ClearList();
        });
    }

    /// <summary>
    /// Playerのコレクターリスト
    /// </summary>
    private List<ICollector> m_FriendList = new List<ICollector>();
    List<ICollector> IUnitHolder.FriendList => m_FriendList;
    ICollector IUnitHolder.Player
    {
        get
        {
            if (m_FriendList.Count != 0)
                return m_FriendList[0];
            else
                return null;
        }
    }
    [ShowNativeProperty]
    private int FriendCount => m_FriendList.Count;

    /// <summary>
    /// Enemyのコレクターリスト
    /// </summary>
    [ReadOnly]
    private List<ICollector> m_EnemyList = new List<ICollector>();
    List<ICollector> IUnitHolder.EnemyList => m_EnemyList;
    [ShowNativeProperty]
    private int EnemyCount => m_EnemyList.Count;

    void IUnitHolder.AddPlayer(ICollector player) => m_FriendList.Add(player);
    void IUnitHolder.AddEnemy(ICollector enemy) => m_EnemyList.Add(enemy);
    void IUnitHolder.RemoveUnit(ICollector unit)
    {
        foreach (ICollector player in m_FriendList)
        {
            if (player == unit)
            {
                m_FriendList.Remove(unit);
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

    /// <summary>
    /// 全てのリストクリア
    /// </summary>
    private void ClearList()
    {
        m_FriendList.Clear();
        m_EnemyList.Clear();
    }
}