using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using NaughtyAttributes;
using UnityEditor;

[CreateAssetMenu(menuName = "MyScriptable/Cleverness/Setup")]
public class ClevernessSetup : ScriptableObject
{
    /// <summary>
    /// 賢さ
    /// </summary>
    [SerializeField, Header("かしこさ"), ReadOnly]
    private List<Cleverness> m_Clevernesses = new List<Cleverness>();
    public ICleverness[] ClevernessEffects => m_Clevernesses.ToArray();

    /// <summary>
    /// 生成する賢さ
    /// </summary>
    [SerializeField, Header("作成するかしこさアセット")]
    [Dropdown("GetEffectType")]
    private string m_Type;

#if UNITY_EDITOR
    /// <summary>
    /// トラップタイプ取得
    /// </summary>
    /// <returns></returns>
    private DropdownList<string> GetEffectType()
    {
        return new DropdownList<string>()
        {
            { "ふみだすゆうき", typeof(CriticalRatioUp).FullName },
            { "ブレイバー・ラゴン", typeof(AttackUp).FullName },
            { "ラゴン・ポイズン", typeof(AttackWithPoison).FullName },
            { "ラゴン・ベノム", typeof(AttackUpIfPoison).FullName },
        };
    }

    /// <summary>
    /// 効果アセットがすでにあるなら消して新しく作り直す
    /// </summary>
    /// <returns></returns>
    [Button]
    private void CreateEffectAsset()
    {
        var t = Type.GetType(m_Type);
        var skill = CreateAssetInternal(t);

        AssetDatabase.AddObjectToAsset(skill, this);
        AssetDatabase.SaveAssets();

        EditorUtility.SetDirty(this);
        EditorUtility.SetDirty(skill);
    }

    /// <summary>
    /// 効果アセットがすでにあるなら消す
    /// </summary>
    /// <returns></returns>
    [Button]
    private void DestroyEffectAsset()
    {
        foreach (var cleverness in m_Clevernesses)
            DestroyImmediate(cleverness, true);

        m_Clevernesses.Clear();
    }

    /// <summary>
    /// 効果アセット作成
    /// </summary>
    /// <param name="type"></param>
    private Cleverness CreateAssetInternal(Type type)
    {
        var cleverness = (Cleverness)ScriptableObject.CreateInstance(type);
        m_Clevernesses.Add(cleverness);
        cleverness.name = type.ToString();
        return cleverness;
    }
#endif
}
