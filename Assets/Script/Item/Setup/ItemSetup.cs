using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using NaughtyAttributes;
using UnityEditor;

[CreateAssetMenu(menuName = "MyScriptable/Item/Setup")]
public class ItemSetup : ScriptableObject
{
    /// <summary>
    /// プレハブ
    /// </summary>
    [SerializeField, Header("プレハブ")]
    private GameObject m_Prefab;
    public GameObject Prefab => m_Prefab;

    /// <summary>
    /// アイテム名
    /// </summary>
    [SerializeField, Header("アイテム名")]
    [ResizableTextArea]
    private string m_ItemName;
    public string ItemName => m_ItemName;

    /// <summary>
    /// アイテム効果
    /// </summary>
    [SerializeField, Header("アイテム効果")]
    [Expandable]
    private ItemEffectBase m_Effect;
    public ItemEffectBase Effect => m_Effect;

    [SerializeField, Header("作成するアイテム効果アセット")]
    [Dropdown("GetItemEffectType")]
    private string m_Type;

    private DropdownList<string> GetItemEffectType()
    {
        return new DropdownList<string>()
        {
            { "UNDEFINE", typeof(SampleItemEffect).FullName },
            { "空腹値回復", typeof(SatisfyHungryDesire).FullName },
        };
    }

    /// <summary>
    /// 効果アセットがすでにあるなら消して新しく作り直す
    /// </summary>
    /// <returns></returns>
    [Button]
    private void CreateItemEffectAsset()
    {
        if (m_Effect != null)
            DestroyImmediate(m_Effect, true);

        var t = Type.GetType(m_Type);
        CreateItemEffectAssetInternal(t);

        AssetDatabase.AddObjectToAsset(m_Effect, this);
        AssetDatabase.SaveAssets();

        EditorUtility.SetDirty(this);
        EditorUtility.SetDirty(m_Effect);
    }

    /// <summary>
    /// 効果アセット作成
    /// </summary>
    /// <param name="type"></param>
    private void CreateItemEffectAssetInternal(Type type)
    {
        // サンプル
        if (type == typeof(SampleItemEffect))
            m_Effect = ScriptableObject.CreateInstance<SampleItemEffect>();

        // 空腹値回復
        if (type == typeof(SatisfyHungryDesire))
            m_Effect = ScriptableObject.CreateInstance<SatisfyHungryDesire>();


        if (m_Effect == null)
            return;

        m_Effect.name = type.ToString();
    }
}