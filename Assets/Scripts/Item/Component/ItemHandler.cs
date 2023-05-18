using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using NaughtyAttributes;

public interface IItemHandler : IDisposable
{
    ItemSetup Setup { get; }
    GameObject ItemObject { get; }

    /// <summary>
    /// アイテム位置
    /// </summary>
    Vector3Int Position { get; }

    /// <summary>
    /// 初期化
    /// </summary>
    void Initialize(ItemSetup setup, GameObject gameObject, Vector3Int pos);

    /// <summary>
    /// インベントリに入ったとき
    /// </summary>
    void OnPut();
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
    private GameObject m_ItemObject;
    GameObject IItemHandler.ItemObject => m_ItemObject;

    /// <summary>
    /// アイテムの位置
    /// </summary>
    [SerializeField, ReadOnly]
    private Vector3Int m_Position;
    Vector3Int IItemHandler.Position => m_Position;

    /// <summary>
    /// 初期化
    /// </summary>
    /// <param name="setup"></param>
    /// <param name="gameObject"></param>
    /// <param name="pos"></param>
    void IItemHandler.Initialize(ItemSetup setup, GameObject gameObject, Vector3Int pos)
    {
        m_Setup = setup;
        m_ItemObject = gameObject;
        m_ItemObject.SetActive(true);
        m_Position = pos;
        m_ItemObject.transform.position = pos + new Vector3(0f, OFFSET_Y, 0f);
    }

    void IItemHandler.OnPut()
    {
        ItemManager.Interface.RemoveItem(this);
        ObjectPoolController.Interface.SetObject(m_Setup, m_ItemObject);
    }

    void IDisposable.Dispose()
    {
        ObjectPoolController.Interface.SetObject(m_Setup, m_ItemObject);
    }
}