using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using System.Linq;

public interface IItemManager : ISingleton
{
    List<IItem> ItemList { get; }
    void AddItem(IItem item);
}

public class ItemManager : Singleton<ItemManager, IItemManager>, IItemManager
{
    /// <summary>
    /// 落ちているアイテムリスト
    /// </summary>
    private List<IItem> m_ItemList = new List<IItem>();
    List<IItem> IItemManager.ItemList => m_ItemList;

    void IItemManager.AddItem(IItem item) => m_ItemList.Add(item);
}
