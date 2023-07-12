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