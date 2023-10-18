using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ListExtension
{
    public static T RandomLottery<T>(this IList<T> list)
    {
        var index = UnityEngine.Random.Range(0, list.Count);
        return list[index];
    }

    public static IList<T> Shuffle<T>(this IList<T> list)
    {
        // リストlistをシャッフルする (for降順ランダム取り)
        for (int i = list.Count - 1; i > 0; i--)
        {
            var j = Random.Range(0, i + 1); // ランダムで要素番号を１つ選ぶ（ランダム要素）
            var temp = list[i]; // 一番最後の要素を仮確保（temp）にいれる
            list[i] = list[j]; // ランダム要素を一番最後にいれる
            list[j] = temp; // 仮確保を元ランダム要素に上書き
        }
        return list;
    }
}
