using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using System.Linq;

public interface IUnitManager : ISingleton
{
    List<ICollector> PlayerList { get; }
    List<ICollector> EnemyList { get; }

    bool TryGetSpecifiedPositionUnit(Vector3 pos, out ICollector val, CHARA_TYPE target = CHARA_TYPE.NONE);

    bool TryGetSpecifiedRoomUnitList(int roomId, out List<ICollector> list, CHARA_TYPE target = CHARA_TYPE.NONE);

    void AddPlayer(ICollector player);
    void AddEnemy(ICollector enemy);
    void RemoveUnit(ICollector unit);

    bool IsUnitOn(Vector3 pos);
    bool IsPlayerOn(Vector3 pos);
    bool IsEnemyOn(Vector3 pos);
}

public class UnitManager : Singleton<UnitManager, IUnitManager>, IUnitManager
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
    private List<ICollector> m_PlayerList = new List<ICollector>();
    List<ICollector> IUnitManager.PlayerList => m_PlayerList;

    /// <summary>
    /// Enemyのコレクターリスト
    /// </summary>
    private List<ICollector> m_EnemyList = new List<ICollector>();
    List<ICollector> IUnitManager.EnemyList => m_EnemyList;

    void IUnitManager.AddPlayer(ICollector player) => m_PlayerList.Add(player);
    void IUnitManager.AddEnemy(ICollector enemy) => m_EnemyList.Add(enemy);
    void IUnitManager.RemoveUnit(ICollector unit)
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

    /// <summary>
    /// 特定の座標のユニットを取得
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    private bool TryGetSpecifiedPositionUnit(Vector3 pos, CHARA_TYPE target, out ICollector val)
    {
        if (target == CHARA_TYPE.PLAYER || target == CHARA_TYPE.NONE)
        {
            foreach (ICollector collector in m_PlayerList)
            {
                if (collector.RequireInterface<ICharaMove>(out var move) == false)
                    continue;

                if (move.Position.x == pos.x && move.Position.z == pos.z)
                {
                    val = collector;
                    return true;
                }
            }
        }

        if (target == CHARA_TYPE.ENEMY || target == CHARA_TYPE.NONE)
        {
            foreach (ICollector collector in m_EnemyList)
            {
                if (collector.RequireInterface<ICharaMove>(out var move) == false)
                    continue;

                if (move.Position.x == pos.x && move.Position.z == pos.z)
                {
                    val = collector;
                    return true;
                }
            }
        }

        val = null;
        return false;
    }

    bool IUnitManager.TryGetSpecifiedPositionUnit(Vector3 pos, out ICollector val, CHARA_TYPE target) => TryGetSpecifiedPositionUnit(pos, target, out val);

    /// <summary>
    /// 特定の部屋にいるプレイヤーを取得
    /// </summary>
    /// <param name="roomId"></param>
    /// <returns></returns>
    private bool TryGetSpecifiedRoomUnitList(int roomId, CHARA_TYPE target, out List<ICollector> val)
    {
        val = new List<ICollector>();
        if (roomId <= 0)
            return false;

        List<ICell> roomList = DungeonManager.Interface.GetRoomCellList(roomId);

        if (target == CHARA_TYPE.PLAYER || target == CHARA_TYPE.NONE)
        {
            foreach (ICollector unit in m_PlayerList)
            {
                if (unit.RequireInterface<ICharaMove>(out var move) == false)
                    continue;

                foreach (ICell cell in roomList)
                    if (move.Position.x == cell.Position.x && move.Position.z == cell.Position.z)
                        val.Add(unit);
            }
        }

        if (target == CHARA_TYPE.ENEMY || target == CHARA_TYPE.NONE)
        {
            foreach (ICollector unit in m_PlayerList)
            {
                if (unit.RequireInterface<ICharaMove>(out var move) == false)
                    continue;

                foreach (ICell cell in roomList)
                    if (move.Position.x == cell.Position.x && move.Position.z == cell.Position.z)
                        val.Add(unit);
            }
        }

        return val.Count != 0;
    }

    bool IUnitManager.TryGetSpecifiedRoomUnitList(int roomId, out List<ICollector> list, CHARA_TYPE target) => TryGetSpecifiedRoomUnitList(roomId, target, out list);

    /// <summary>
    /// ユニットが存在するかを返す
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    bool IUnitManager.IsUnitOn(Vector3 pos)
    {
        if (IsPlayerOn(pos) == true)
            return true;

        if (IsEnemyOn(pos) == true)
            return true;

        return false;
    }

    /// <summary>
    /// プレイヤーが存在するかを返す
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    private bool IsPlayerOn(Vector3 pos)
    {
        int pos_x = (int)pos.x;
        int pos_z = (int)pos.z;

        foreach (ICollector player in m_PlayerList)
        {
            var move = player.GetInterface<ICharaMove>();
            if (move.Position.x == pos.x && move.Position.z == pos.z)
                return true;
        }
        return false;
    }

    bool IUnitManager.IsPlayerOn(Vector3 pos) => IsPlayerOn(pos);

    /// <summary>
    /// 敵が存在するかを返す
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    private bool IsEnemyOn(Vector3 pos) //指定座標に敵がいるかどうかを調べる
    {
        int pos_x = (int)pos.x;
        int pos_z = (int)pos.z;

        foreach (ICollector enemy in m_EnemyList)
        {
            var move = enemy.GetInterface<ICharaMove>();
            if (move.Position.x == pos.x && move.Position.z == pos.z)
                return true;
        }
        return false;
    }

    bool IUnitManager.IsEnemyOn(Vector3 pos) => IsEnemyOn(pos);
}
