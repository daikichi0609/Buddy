using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public interface IItem
{
    Define.ITEM_NAME Name { get; }
    GameObject GameObject { get; }
    Vector3 Position { get; set; }
}

public class Item : MonoBehaviour, IItem
{
    /// <summary>
    /// アイテム名
    /// </summary>
    protected Define.ITEM_NAME m_Name;
    Define.ITEM_NAME IItem.Name => m_Name;

    [SerializeField]
    private GameObject m_GameObject;
    GameObject IItem.GameObject => m_GameObject;

    /// <summary>
    /// アイテムの位置
    /// </summary>
    private Vector3 m_Position;
    Vector3 IItem.Position
    {
        get => m_Position;
        set => m_Position = value;
    }

    /// <summary>
    /// アイテム効果
    /// </summary>
    public virtual System.Action Method
    {
        get;
    }
}