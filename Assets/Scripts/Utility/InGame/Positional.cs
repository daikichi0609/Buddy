using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;

public enum DIRECTION
{
    LOWER_LEFT = 0,
    LEFT = 1,
    UPPER_LEFT = 2,
    UNDER = 3,
    UP = 4,
    LOWER_RIGHT = 5,
    RIGHT = 6,
    UPPER_RIGHT = 7,

    MAX = 8,
    NONE = -1,
}

public static class Positional
{
    /// <summary>
    /// V3 -> V3Int
    /// </summary>
    /// <param name="v3"></param>
    /// <returns></returns>
    public static Vector3Int ToV3Int(this Vector3 v3) => new Vector3Int((int)v3.x, (int)v3.y, (int)v3.z);

    /// <summary>
    /// Direction配列
    /// </summary>
    public static readonly Vector3Int[] Directions = new Vector3Int[8]
    {
        new Vector3Int(-1, 0, -1), new Vector3Int(-1, 0, 0), new Vector3Int(-1, 0, 1), new Vector3Int(0, 0, -1),
        new Vector3Int(0, 0, 1), new Vector3Int(1, 0, -1), new Vector3Int(1, 0, 0), new Vector3Int(1, 0, 1)
    };

    /// <summary>
    /// DIRECTION取得
    /// </summary>
    /// <param name="x"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    public static DIRECTION GetDirection(int x, int z)
    {
        if (x == -1)
            return z switch
            {
                -1 => DIRECTION.LOWER_LEFT,
                0 => DIRECTION.LEFT,
                1 => DIRECTION.UPPER_LEFT,
                _ => DIRECTION.NONE,
            };

        else if (x == 0)
            return z switch
            {
                -1 => DIRECTION.UNDER,
                0 => DIRECTION.NONE,
                1 => DIRECTION.UP,
                _ => DIRECTION.NONE,
            };

        else if (x == 1)
            return z switch
            {
                -1 => DIRECTION.LOWER_RIGHT,
                0 => DIRECTION.RIGHT,
                1 => DIRECTION.UPPER_RIGHT,
                _ => DIRECTION.NONE,
            };

        return DIRECTION.NONE;
    }

    /// <summary>
    /// 斜めであるかどうか
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    public static bool IsDiagonal(this DIRECTION dir)
    {
        return dir switch
        {
            DIRECTION.LOWER_LEFT or DIRECTION.LOWER_RIGHT or DIRECTION.UPPER_LEFT or DIRECTION.UPPER_RIGHT => true,
            _ => false
        };
    }

    /// <summary>
    /// Enum -> V3Int
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    public static Vector3Int ToV3Int(this DIRECTION dir)
    {
        switch (dir)
        {
            case DIRECTION.NONE:
            case DIRECTION.MAX:
                return new Vector3Int(0, 0, 0);
        }

        return Directions[(int)dir];
    }

    /// <summary>
    /// V3Int -> Enum
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    public static DIRECTION ToDirEnum(this Vector3Int dir)
    {
        for (int i = 0; i < (int)DIRECTION.MAX; i++)
            if (dir == Directions[i])
                return (DIRECTION)i;

        return DIRECTION.NONE;
    }
    /// <summary>
    /// 反対方向
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    public static Vector3 ToOppositeDir(this Vector3 dir) => dir * -1;
    public static Vector3Int ToOppositeDir(this Vector3Int dir) => dir * -1;
    public static DIRECTION ToOppositeDir(this DIRECTION dir)
    {
        var v3 = dir.ToV3Int();
        return v3.ToOppositeDir().ToDirEnum();
    }

    public static DIRECTION[] NearDirection(this DIRECTION dir)
    {
        return dir switch
        {
            DIRECTION.LOWER_LEFT => new DIRECTION[2] { DIRECTION.LEFT, DIRECTION.UNDER },
            DIRECTION.LEFT => new DIRECTION[2] { DIRECTION.LOWER_LEFT, DIRECTION.UPPER_LEFT },
            DIRECTION.UPPER_LEFT => new DIRECTION[2] { DIRECTION.LEFT, DIRECTION.UP },
            DIRECTION.UP => new DIRECTION[2] { DIRECTION.UPPER_LEFT, DIRECTION.UPPER_RIGHT },
            DIRECTION.UPPER_RIGHT => new DIRECTION[2] { DIRECTION.UP, DIRECTION.RIGHT },
            DIRECTION.RIGHT => new DIRECTION[2] { DIRECTION.UPPER_RIGHT, DIRECTION.LOWER_RIGHT },
            DIRECTION.LOWER_RIGHT => new DIRECTION[2] { DIRECTION.RIGHT, DIRECTION.UNDER },
            DIRECTION.UNDER => new DIRECTION[2] { DIRECTION.LOWER_RIGHT, DIRECTION.LOWER_LEFT },
            _ => new DIRECTION[1] { DIRECTION.NONE },
        };
    }

    /// <summary>
    /// 目的地に向かう正規ベクトルを求める
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="opp"></param>
    /// <returns></returns>
    public static DIRECTION CalculateNormalDirection(this Vector3Int pos, Vector3Int opp)
    {
        var dir = opp - pos;
        if (dir.x == 0 && dir.z == 0)
            return DIRECTION.NONE;

        var x = dir.x;
        var z = dir.z;

        var x_Abs = Mathf.Abs(x);
        var z_Abs = Mathf.Abs(z);

        if (x_Abs == 0)
            return new Vector3Int(0, 0, (int)Mathf.Clamp(z, -1, 1)).ToDirEnum();
        else if (z_Abs == 0)
            return new Vector3Int((int)Mathf.Clamp(x, -1, 1), 0, 0).ToDirEnum();
        else
            return new Vector3Int((int)Mathf.Clamp(x, -1, 1), 0, (int)Mathf.Clamp(z, -1, 1)).ToDirEnum();
    }

    /// <summary>
    /// マンハッタン距離
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="opp"></param>
    /// <returns></returns>
    public static int CalculateManhattanDistance(this Vector3Int pos, Vector3Int opp)
    {
        return Math.Abs(pos.x - opp.x) + Math.Abs(pos.z - opp.z);
    }

    public static bool TryGetForwardUnit(Vector3Int pos, Vector3Int dir, int distance, CHARA_TYPE target, IDungeonHandler dungeonHandler, IUnitFinder unitFinder, out ICollector hit, out int flyDistance)
    {
        hit = null; // ヒットしたキャラ

        for (flyDistance = 1; flyDistance <= distance; flyDistance++)
        {
            // 攻撃マス
            var targetPos = pos + dir * flyDistance;

            // 攻撃対象ユニットが存在するか調べる
            if (unitFinder.TryGetSpecifiedPositionUnit(targetPos, out hit, target) == true)
                break;

            // 地形チェック
            var terrain = dungeonHandler.GetCellId(targetPos);
            // 壁だったら走査終了
            if (terrain == TERRAIN_ID.WALL)
            {
                flyDistance--; // 手前まで飛ぶ
                break;
            }
        }

        return hit != null;
    }
}