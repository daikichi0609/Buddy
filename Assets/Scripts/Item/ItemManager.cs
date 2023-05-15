using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using System.Linq;

public interface IItemManager : ISingleton
{
    List<IItemHandler> ItemList { get; }
    void AddItem(IItemHandler item);
    void RemoveItem(IItemHandler item);
}

public class ItemManager : Singleton<ItemManager, IItemManager>, IItemManager
{
    protected override void Awake()
    {
        base.Awake();
        DungeonContentsDeployer.Interface.OnRemoveContents.Subscribe(_ =>
        {
            m_ItemList.Clear();
        });
    }

    /// <summary>
    /// 落ちているアイテムリスト
    /// </summary>
    private List<IItemHandler> m_ItemList = new List<IItemHandler>();
    List<IItemHandler> IItemManager.ItemList => m_ItemList;

    void IItemManager.AddItem(IItemHandler item) => m_ItemList.Add(item);
    void IItemManager.RemoveItem(IItemHandler item) => m_ItemList.Remove(item);
}
