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
    UP = 3,
    UPPER_RIGHT = 4,
    RIGHT = 5,
    LOWER_RIGHT = 6,
    UNDER = 7,

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
    private static readonly Vector3Int[] Direction = new Vector3Int[8]
    {
        new Vector3Int(-1, 0, -1), new Vector3Int(-1, 0, 0), new Vector3Int(-1, 0, 1), new Vector3Int(0, 0, 1),
        new Vector3Int(1, 0, 1), new Vector3Int(1, 0, 0), new Vector3Int(1, 0, -1),  new Vector3Int(0, 0, -1),
    };

    /// <summary>
    /// Enum -> V3Int
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    public static Vector3Int ToV3Int(this DIRECTION dir) => Direction[(int)dir];

    /// <summary>
    /// V3Int -> Enum
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    public static DIRECTION ToDirEnum(this Vector3Int dir)
    {
        for (int i = 0; i < (int)DIRECTION.MAX; i++)
            if (dir == Direction[i])
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
        int num = (int)dir;

        int low = num - 1;
        if (low < 0)
            low = (int)DIRECTION.MAX - 1;

        int high = num + 1;
        if (high >= (int)DIRECTION.MAX)
            high = 0;

        return new DIRECTION[2] { (DIRECTION)low, (DIRECTION)high };
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
}