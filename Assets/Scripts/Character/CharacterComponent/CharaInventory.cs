using UnityEngine;
using UniRx;
using System;
using System.Collections.Generic;
using NaughtyAttributes;

public interface ICharaInventory : IActorInterface
{
    /// <summary>
    /// 所持しているアイテム
    /// </summary>
    IItemHandler[] Items { get; }

    /// <summary>
    /// アイテムをしまう
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    bool Put(IItemHandler item, IDisposable disposable);

    /// <summary>
    /// アイテムを消費する
    /// </summary>
    /// <param name="item"></param>
    void Consume(IItemHandler item);
}

public interface ICharaInventoryEvent : IActorInterface
{
    /// <summary>
    /// アイテムしまうとき
    /// </summary>
    IObservable<ItemPutInfo> OnPutItem { get; }
}

public readonly struct ItemPutInfo
{
    public ICollector Owner { get; }
    public IItemHandler Item { get; }

    public ItemPutInfo(ICollector owner, IItemHandler item)
    {
        Owner = owner;
        Item = item;
    }
}

public class CharaInventory : ActorComponentBase, ICharaInventory, ICharaInventoryEvent
{
    private static readonly int InventoryCount = 9;

    private List<IItemHandler> m_ItemList = new List<IItemHandler>();
    IItemHandler[] ICharaInventory.Items => m_ItemList.ToArray();

    Subject<ItemPutInfo> m_OnPutItem = new Subject<ItemPutInfo>();
    IObservable<ItemPutInfo> ICharaInventoryEvent.OnPutItem => m_OnPutItem;

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
    bool ICharaInventory.Put(IItemHandler item, IDisposable disposable)
    {
        disposable.Dispose();

        if (m_ItemList.Count < InventoryCount)
        {
            m_ItemList.Add(item);
            item.OnPut();
            m_OnPutItem.OnNext(new ItemPutInfo(Owner, item));
            return true;
        }
        else
        {
#if DEBUG
            Debug.Log("アイテムがいっぱいです");
#endif
            return false;
        }
    }

    /// <summary>
    /// アイテムを消費する
    /// </summary>
    void ICharaInventory.Consume(IItemHandler item)
    {
        m_ItemList.Remove(item);
    }
}