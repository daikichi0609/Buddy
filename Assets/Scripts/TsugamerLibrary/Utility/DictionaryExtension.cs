using System.Collections.Generic;

public static class DictionaryExtension
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