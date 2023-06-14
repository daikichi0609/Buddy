using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
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
    /// 空腹値
    /// </summary>
    [SerializeField, Header("空腹値")]
    private int m_Hugry;
    public int Hugry => m_Hugry;

    /// <summary>
    /// アイテム
    /// </summary>
    [SerializeField, Header("アイテム")]
    private ItemSetup[] m_Items;
    public ItemSetup[] Items => m_Items;

    public void WriteData(float exp, int hungry, ItemSetup[] items)
    {
        m_Exp = exp;
        m_Hugry = hungry;
        m_Items = items;
    }
}
