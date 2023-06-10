using UnityEngine;
using UniRx;
using System;
using System.Collections.Generic;
using NaughtyAttributes;
using Zenject;

public interface ICharaInventory : IActorInterface
{
    /// <summary>
    /// アイテムをしまう
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    void Put(IItemHandler item, IDisposable disposable);
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
    [Inject]
    private ITeamInventory m_TeamInventory;

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
    void ICharaInventory.Put(IItemHandler item, IDisposable disposable)
    {
        disposable.Dispose();

        bool put = m_TeamInventory.TryPut(item);
        if (put == true)
        {
            item.OnPut();
            m_OnPutItem.OnNext(new ItemPutInfo(Owner, item));
        }
    }
}