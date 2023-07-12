using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using System.Linq;
using Zenject;

public interface IUnitFinder
{
    /// <summary>
    /// 特定座標のユニット取得
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="val"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    bool TryGetSpecifiedPositionUnit(Vector3 pos, out ICollector val, CHARA_TYPE target = CHARA_TYPE.NONE);

    /// <summary>
    /// 特定の部屋のユニット取得
    /// </summary>
    /// <param name="roomId"></param>
    /// <param name="list"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    bool TryGetSpecifiedRoomUnitList(int roomId, out ICollector[] targets, CHARA_TYPE target = CHARA_TYPE.NONE);

    /// <summary>
    /// ユニットいるか
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    bool IsUnitOn(Vector3Int pos, CHARA_TYPE target = CHARA_TYPE.NONE);
}

public class UnitFinder : IUnitFinder
{
    [Inject]
    private IDungeonDeployer m_DungeonDeployer;
    [Inject]
    private IUnitHolder m_UnitHolder;

    /// <summary>
    /// 特定の座標のユニットを取得
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    private bool TryGetSpecifiedPositionUnit(Vector3 pos, CHARA_TYPE target, out ICollector val)
    {
        if (target == CHARA_TYPE.FRIEND || target == CHARA_TYPE.NONE)
        {
            foreach (ICollector collector in m_UnitHolder.FriendList)
            {
                if (collector.RequireInterface<ICharaMove>(out var move) == false)
                    continue;

                if (move.Position == pos)
                {
                    val = collector;
                    return true;
                }
            }
        }

        if (target == CHARA_TYPE.ENEMY || target == CHARA_TYPE.NONE)
        {
            foreach (ICollector collector in m_UnitHolder.EnemyList)
            {
                if (collector.RequireInterface<ICharaMove>(out var move) == false)
                    continue;

                if (move.Position == pos)
                {
                    val = collector;
                    return true;
                }
            }
        }

        val = null;
        return false;
    }

    bool IUnitFinder.TryGetSpecifiedPositionUnit(Vector3 pos, out ICollector val, CHARA_TYPE target) => TryGetSpecifiedPositionUnit(pos, target, out val);

    /// <summary>
    /// 特定の部屋にいるプレイヤーを取得
    /// </summary>
    /// <param name="roomId"></param>
    /// <returns></returns>
    private bool TryGetSpecifiedRoomUnitList(int roomId, CHARA_TYPE target, out ICollector[] targets)
    {
        var list = new List<ICollector>();
        targets = null;
        if (roomId < 0)
            return false;

        var roomList = m_DungeonDeployer.GetRoom(roomId).Cells;

        if (target == CHARA_TYPE.FRIEND || target == CHARA_TYPE.NONE)
        {
            foreach (ICollector unit in m_UnitHolder.FriendList)
            {
                if (unit.RequireInterface<ICharaMove>(out var move) == false)
                    continue;

                foreach (var cell in roomList)
                    if (cell.RequireInterface<ICellInfoHandler>(out var info) == true)
                        if (move.Position == info.Position)
                            list.Add(unit);
            }
        }

        if (target == CHARA_TYPE.ENEMY || target == CHARA_TYPE.NONE)
        {
            foreach (ICollector unit in m_UnitHolder.EnemyList)
            {
                if (unit.RequireInterface<ICharaMove>(out var move) == false)
                    continue;

                foreach (var cell in roomList)
                    if (cell.RequireInterface<ICellInfoHandler>(out var info) == true)
                        if (move.Position == info.Position)
                            list.Add(unit);
            }
        }

        targets = list.ToArray();
        return targets.Length != 0;
    }

    bool IUnitFinder.TryGetSpecifiedRoomUnitList(int roomId, out ICollector[] targets, CHARA_TYPE target) => TryGetSpecifiedRoomUnitList(roomId, target, out targets);

    /// <summary>
    /// ユニットが存在するかを返す
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    bool IUnitFinder.IsUnitOn(Vector3Int pos, CHARA_TYPE target)
    {
        if (target == CHARA_TYPE.NONE || target == CHARA_TYPE.FRIEND)
            if (IsFriendOn(pos) == true)
                return true;

        if (target == CHARA_TYPE.NONE || target == CHARA_TYPE.ENEMY)
            if (IsEnemyOn(pos) == true)
                return true;

        return false;
    }

    /// <summary>
    /// プレイヤーが存在するかを返す
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    private bool IsFriendOn(Vector3Int pos)
    {
        foreach (ICollector player in m_UnitHolder.FriendList)
        {
            var move = player.GetInterface<ICharaMove>();
            if (move.Position == pos)
                return true;
        }
        return false;
    }

    /// <summary>
    /// 敵が存在するかを返す
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    private bool IsEnemyOn(Vector3Int pos) //指定座標に敵がいるかどうかを調べる
    {
        foreach (ICollector enemy in m_UnitHolder.EnemyList)
        {
            var move = enemy.GetInterface<ICharaMove>();
            if (move.Position == pos)
                return true;
        }
        return false;
    }
}
