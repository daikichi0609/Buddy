using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using NaughtyAttributes;

public interface IItemHandler
{
    ItemSetup Setup { get; }
    GameObject ItemObject { get; }

    /// <summary>
    /// アイテム位置
    /// </summary>
    Vector3Int Position { get; set; }

    /// <summary>
    /// 初期化
    /// </summary>
    void Initialize(ItemSetup setup, GameObject gameObject);
}

public class ItemHandler : MonoBehaviour, IItemHandler
{
    public static readonly float OFFSET_Y = 0.75f;

    /// <summary>
    /// アイテム名
    /// </summary>
    [SerializeField, Header("セットアップ")]
    [ReadOnly, Expandable]
    private ItemSetup m_Setup;
    public ItemSetup Setup => m_Setup;

    /// <summary>
    /// ゲームオブジェクト
    /// </summary>
    [SerializeField]
    private GameObject m_ItemObject;
    GameObject IItemHandler.ItemObject => m_ItemObject;

    /// <summary>
    /// アイテムの位置
    /// </summary>
    [SerializeField, ReadOnly]
    private Vector3Int m_Position;
    Vector3Int IItemHandler.Position
    {
        get => m_Position;
        set => m_Position = value;
    }

    void IItemHandler.Initialize(ItemSetup setup, GameObject gameObject)
    {
        m_Setup = setup;
        m_ItemObject = gameObject;
    }
}