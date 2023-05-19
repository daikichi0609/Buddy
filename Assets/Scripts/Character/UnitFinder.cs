using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using System.Linq;

public interface IUnitFinder : ISingleton
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
    bool TryGetSpecifiedRoomUnitList(int roomId, out List<ICollector> list, CHARA_TYPE target = CHARA_TYPE.NONE);

    /// <summary>
    /// ユニットいるか
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    bool IsUnitOn(Vector3Int pos);

    /// <summary>
    /// プレイヤーいるか
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    bool IsPlayerOn(Vector3Int pos);

    /// <summary>
    /// 敵いるか
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    bool IsEnemyOn(Vector3Int pos);
}

public class UnitFinder : Singleton<UnitFinder, IUnitFinder>, IUnitFinder
{
    /// <summary>
    /// 特定の座標のユニットを取得
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    private bool TryGetSpecifiedPositionUnit(Vector3 pos, CHARA_TYPE target, out ICollector val)
    {
        if (target == CHARA_TYPE.PLAYER || target == CHARA_TYPE.NONE)
        {
            foreach (ICollector collector in UnitHolder.Interface.FriendList)
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
            foreach (ICollector collector in UnitHolder.Interface.EnemyList)
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
    private bool TryGetSpecifiedRoomUnitList(int roomId, CHARA_TYPE target, out List<ICollector> val)
    {
        val = new List<ICollector>();
        if (roomId < 0)
            return false;

        var roomList = DungeonDeployer.Interface.GetRoom(roomId).Cells;

        if (target == CHARA_TYPE.PLAYER || target == CHARA_TYPE.NONE)
        {
            foreach (ICollector unit in UnitHolder.Interface.FriendList)
            {
                if (unit.RequireInterface<ICharaMove>(out var move) == false)
                    continue;

                foreach (var cell in roomList)
                    if (cell.RequireInterface<ICellInfoHandler>(out var info) == true)
                        if (move.Position == info.Position)
                            val.Add(unit);
            }
        }

        if (target == CHARA_TYPE.ENEMY || target == CHARA_TYPE.NONE)
        {
            foreach (ICollector unit in UnitHolder.Interface.EnemyList)
            {
                if (unit.RequireInterface<ICharaMove>(out var move) == false)
                    continue;

                foreach (var cell in roomList)
                    if (cell.RequireInterface<ICellInfoHandler>(out var info) == true)
                        if (move.Position == info.Position)
                            val.Add(unit);
            }
        }

        return val.Count != 0;
    }

    bool IUnitFinder.TryGetSpecifiedRoomUnitList(int roomId, out List<ICollector> list, CHARA_TYPE target) => TryGetSpecifiedRoomUnitList(roomId, target, out list);

    /// <summary>
    /// ユニットが存在するかを返す
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    bool IUnitFinder.IsUnitOn(Vector3Int pos)
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
    private bool IsPlayerOn(Vector3Int pos)
    {
        foreach (ICollector player in UnitHolder.Interface.FriendList)
        {
            var move = player.GetInterface<ICharaMove>();
            if (move.Position == pos)
                return true;
        }
        return false;
    }

    bool IUnitFinder.IsPlayerOn(Vector3Int pos) => IsPlayerOn(pos);

    /// <summary>
    /// 敵が存在するかを返す
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    private bool IsEnemyOn(Vector3Int pos) //指定座標に敵がいるかどうかを調べる
    {
        foreach (ICollector enemy in UnitHolder.Interface.EnemyList)
        {
            var move = enemy.GetInterface<ICharaMove>();
            if (move.Position == pos)
                return true;
        }
        return false;
    }

    bool IUnitFinder.IsEnemyOn(Vector3Int pos) => IsEnemyOn(pos);
}
