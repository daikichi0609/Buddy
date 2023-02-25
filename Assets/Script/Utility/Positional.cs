using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

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
    public static Vector3Int ToV3Int(this Vector3 v3) => new Vector3Int((int)v3.x, (int)v3.y, (int)v3.z);

    private static readonly Vector3Int[] Direction = new Vector3Int[8]
    {
        new Vector3Int(-1, 0, -1), new Vector3Int(-1, 0, 0), new Vector3Int(-1, 0, 1), new Vector3Int(0, 0, 1),
        new Vector3Int(1, 0, 1), new Vector3Int(1, 0, 0), new Vector3Int(1, 0, -1),  new Vector3Int(0, 0, -1),
    };

    public static Vector3Int ToV3Int(this DIRECTION dir) => Direction[(int)dir];
    public static DIRECTION ToDirEnum(this Vector3Int dir)
    {
        for (int i = 0; i < (int)DIRECTION.MAX; i++)
            if (dir == Direction[i])
                return (DIRECTION)i;

        return DIRECTION.NONE;
    }

    public static DIRECTION CalculateDirection(this Vector3Int pos, Vector3 opp)
    {
        var dir = opp - pos;
        if (dir.x == 0 && dir.z == 0)
            return DIRECTION.NONE;

        var x = dir.x;
        var z = dir.z;

        var x_Abs = Mathf.Abs(x);
        var z_Abs = Mathf.Abs(z);

        if (x_Abs == z_Abs)
            return new Vector3Int((int)Mathf.Clamp(x, -1, 1), 0, (int)Mathf.Clamp(z, -1, 1)).ToDirEnum();
        else if (x_Abs > z_Abs)
            return new Vector3Int((int)Mathf.Clamp(x, -1, 1), 0, 0).ToDirEnum();
        else
            return new Vector3Int(0, 0, (int)Mathf.Clamp(z, -1, 1)).ToDirEnum();
    }
}