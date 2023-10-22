using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "MyScriptable/Dungeon/CharacterProgress")]
public class DungeonCharacterProgressSaveData : ScriptableObject
{
    /// <summary>
    /// 経験値
    /// </summary>
    [SerializeField, Header("チーム経験値")]
    private float m_Exp;
    public float Exp => m_Exp;

    /// <summary>
    /// アイテム
    /// </summary>
    [SerializeField, Header("アイテム")]
    private ItemSetup[] m_Items;
    public ItemSetup[] Items => m_Items;

    public void WriteData(float exp, ItemSetup[] items)
    {
        m_Exp = exp;
        m_Items = items;
    }

    /// <summary>
    /// 進捗リセット
    /// </summary>
    [Button]
    public void ResetData()
    {
        m_Exp = 0;
        m_Items = null;

        EditorUtility.SetDirty(this);
    }
}
