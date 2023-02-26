using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public interface IItem
{
    ITEM_NAME Name { get; }
    GameObject ItemObject { get; }
    Vector3Int Position { get; set; }
}

public class Item : MonoBehaviour, IItem
{
    /// <summary>
    /// アイテム名
    /// </summary>
    protected ITEM_NAME m_Name;
    ITEM_NAME IItem.Name => m_Name;

    [SerializeField]
    private GameObject m_ItemObject;
    GameObject IItem.ItemObject => m_ItemObject;

    /// <summary>
    /// アイテムの位置
    /// </summary>
    private Vector3Int m_Position;
    Vector3Int IItem.Position
    {
        get => m_Position;
        set => m_Position = value;
    }
}