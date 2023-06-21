using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using NaughtyAttributes;
using UnityEditor;

[CreateAssetMenu(menuName = "MyScriptable/Item/Setup")]
public class ItemSetup : PrefabSetup
{
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
    public IItemEffect Effect => m_Effect;

    /// <summary>
    /// 食べられるか
    /// </summary>
    [SerializeField, Header("食べられるか")]
    private bool m_CanEat;
    public bool CanEat => m_CanEat;

    [SerializeField, Header("作成するアイテム効果アセット")]
    [Dropdown("GetItemEffectType")]
    private string m_Type;

#if UNITY_EDITOR

    private DropdownList<string> GetItemEffectType()
    {
        return new DropdownList<string>()
        {
            { "UNDEFINE", typeof(SampleItemEffect).FullName },
            { "空腹値回復", typeof(SatisfyHungryDesire).FullName },
            { "固定ダメージ", typeof(CauseFixedDamage).FullName },
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

        // 直線投擲
        if (type == typeof(CauseFixedDamage))
            m_Effect = ScriptableObject.CreateInstance<CauseFixedDamage>();


        if (m_Effect == null)
            return;

        m_Effect.name = type.ToString();
    }
#endif
}