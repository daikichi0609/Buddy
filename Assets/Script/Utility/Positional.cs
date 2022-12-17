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
    private static readonly Vector3[] Direction = new Vector3[8]
    {
        new Vector3(-1f, 0f, -1f), new Vector3(-1f, 0f, 0f), new Vector3(-1f, 0f, 1f), new Vector3(0f, 0f, 1f),
        new Vector3(1f, 0f, 1f), new Vector3(1f, 0f, 0f), new Vector3(1f, 0f, -1f),  new Vector3(0f, 0f, -1f),
    };

    public static Vector3 GetDirection(DIRECTION dir) => Direction[(int)dir];
    public static DIRECTION GetDirection(Vector3 dir)
    {
        for (int i = 0; i < (int)DIRECTION.MAX; i++)
            if (dir == Direction[i])
                return (DIRECTION)i;

        return DIRECTION.NONE;
    }

    public static Vector3 CalculateDirection(Vector3 pos, Vector3 opp)
    {
        var dir = opp - pos;
        if (dir.x == 0 && dir.z == 0)
            return new Vector3(0f, 0f, 0f);

        var x = dir.x;
        var z = dir.z;

        var x_Abs = Mathf.Abs(x);
        var z_Abs = Mathf.Abs(z);

        if (x_Abs == z_Abs)
            return new Vector3(Mathf.Clamp(x, -1f, 1f), 0f, Mathf.Clamp(z, -1f, 1f));
        else if (x_Abs > z_Abs)
            return new Vector3(Mathf.Clamp(x, -1f, 1f), 0f, 0f);
        else
            return new Vector3(0f, 0f, Mathf.Clamp(z, -1f, 1f));
    }
}