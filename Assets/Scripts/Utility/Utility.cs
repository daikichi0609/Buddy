using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;

public static class Utility
{
    public static T RandomLottery<T>(this IList<T> list)
    {
        var index = UnityEngine.Random.Range(0, list.Count);
        return list[index];
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

public static class Coroutine
{
    public static IEnumerator DelayCoroutine<T>(float seconds, T state, System.Action<T> action)
    {
        yield return new WaitForSeconds(seconds);
        action?.Invoke(state);
    }
}

public static class DictionaryExtensions
{
    /// <summary>
    /// 値を取得、keyがなければデフォルト値を設定し、デフォルト値を取得
    /// </summary>
    public static TV GetOrDefault<TK, TV>(this Dictionary<TK, TV> dic, TK key, TV defaultValue = default(TV))
    {
        if (dic.ContainsKey(key) == false)
            dic.Add(key, defaultValue);

        return dic[key];
    }
}