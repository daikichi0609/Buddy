using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using NaughtyAttributes;
using UnityEditor;

[CreateAssetMenu(menuName = "MyScriptable/Trap/Setup")]
public class TrapSetup : PrefabSetup
{
    /// <summary>
    /// トラップ名
    /// </summary>
    [SerializeField, Header("トラップ名")]
    [ResizableTextArea]
    private string m_TrapName;
    public string TrapName => m_TrapName;

    /// <summary>
    /// エフェクト
    /// </summary>
    [SerializeField, Header("エフェクトプレハブ")]
    private GameObject m_EffectObject;
    public GameObject EffectObject => m_EffectObject;

    /// <summary>
    /// トラップ効果
    /// </summary>
    [SerializeField, Header("トラップ効果")]
    [Expandable]
    private TrapEffectBase m_TrapEffect;
    public ITrap TrapEffect => m_TrapEffect;

    /// <summary>
    /// 生成するトラップタイプ
    /// </summary>
    [SerializeField, Header("作成するトラップ効果アセット")]
    [Dropdown("GetTrapEffectType")]
    private string m_Type;

#if UNITY_EDITOR
    /// <summary>
    /// トラップタイプ取得
    /// </summary>
    /// <returns></returns>
    private DropdownList<string> GetTrapEffectType()
    {
        return new DropdownList<string>()
        {
            { "UNDEFINE", typeof(SampleTrapEffect).FullName },
            { "爆発範囲ダメージ", typeof(BombTrap).FullName },
            { "毒", typeof(PoisonTrap).FullName },
            { "眠り", typeof(SleepTrap).FullName },
        };
    }

    /// <summary>
    /// 効果アセットがすでにあるなら消して新しく作り直す
    /// </summary>
    /// <returns></returns>
    [Button]
    private void CreateTrapEffectAsset()
    {
        if (m_TrapEffect != null)
            DestroyImmediate(m_TrapEffect, true);

        CreateTrapEffectAssetInternal(Type.GetType(m_Type));

        AssetDatabase.AddObjectToAsset(m_TrapEffect, this);
        AssetDatabase.SaveAssets();

        EditorUtility.SetDirty(this);
        EditorUtility.SetDirty(m_TrapEffect);
    }

    /// <summary>
    /// 効果アセット作成
    /// </summary>
    /// <param name="type"></param>
    private void CreateTrapEffectAssetInternal(Type type)
    {
        m_TrapEffect = (TrapEffectBase)ScriptableObject.CreateInstance(type);
        m_TrapEffect.name = type.ToString();
    }
#endif
}
