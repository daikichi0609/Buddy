using UnityEngine;
using UniRx;
using System;
using System.Collections.Generic;
using NaughtyAttributes;

public interface ICharaInventory : ICharacterComponent
{
    /// <summary>
    /// 所持しているアイテム
    /// </summary>
    IItem[] Items { get; }

    /// <summary>
    /// アイテムをしまう
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    bool Put(IItem item);

    /// <summary>
    /// アイテムを消費する
    /// </summary>
    /// <param name="item"></param>
    void Consume(IItem item);
}

public interface ICharaInventoryEvent : ICharacterComponent
{
    /// <summary>
    /// アイテムしまうとき
    /// </summary>
    IObservable<(ICollector, IItem)> OnPutItem { get; }
}

public class CharaInventory : CharaComponentBase, ICharaInventory, ICharaInventoryEvent
{
    private static readonly int InventoryCount = 9;

    private List<IItem> m_ItemList = new List<IItem>();
    IItem[] ICharaInventory.Items => m_ItemList.ToArray();

    Subject<(ICollector, IItem)> m_OnPutItem = new Subject<(ICollector, IItem)>();
    IObservable<(ICollector, IItem)> ICharaInventoryEvent.OnPutItem => m_OnPutItem;

    protected override void Register(ICollector owner)
    {
        base.Register(owner);
        owner.Register<ICharaInventory>(this);
        owner.Register<ICharaInventoryEvent>(this);
    }

    /// <summary>
    /// アイテムをしまう
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    bool ICharaInventory.Put(IItem item)
    {
        if (m_ItemList.Count < InventoryCount)
        {
            m_ItemList.Add(item);
            ObjectPool.Instance.SetObject(item.Name.ToString(), item.ItemObject);
            ItemManager.Interface.RemoveItem(item);

            m_OnPutItem.OnNext((Owner, item));
            return true;
        }
        else
        {
            Debug.Log("アイテムがいっぱいです");
            return false;
        }
    }

    /// <summary>
    /// アイテムを消費する
    /// </summary>
    void ICharaInventory.Consume(IItem item)
    {
        m_ItemList.Remove(item);
    }
}