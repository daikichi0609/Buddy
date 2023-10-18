using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ColorExtension
{
    public static bool IsSameColor(this Color32 color, Color32 other)
    {
        if (color.a != other.a)
            return false;

        if (color.b != other.b)
            return false;

        if (color.g != other.g)
            return false;

        if (color.r != other.r)
            return false;

        return true;
    }
}
