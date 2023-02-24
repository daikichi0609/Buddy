using UnityEngine;
using UniRx;
using System;
using System.Collections.Generic;

public interface ICharaInventory : ICharacterComponent
{
    bool PutAway(IItem item);
}

public class CharaInventory : CharaComponentBase, ICharaInventory
{
    private static readonly int InventoryCount = 9;

    public List<IItem> ItemList { get; } = new List<IItem>();

    protected override void Register(ICollector owner)
    {
        base.Register(owner);
        owner.Register<ICharaInventory>(this);
    }

    /// <summary>
    /// アイテムをしまう
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    bool ICharaInventory.PutAway(IItem item)
    {
        if (ItemList.Count < InventoryCount)
        {
            ItemList.Add(item);
            ObjectPool.Instance.SetObject(item.Name.ToString(), item.GameObject);
            return true;
        }
        else
        {
            Debug.Log("アイテムがいっぱいです");
            return false;
        }
    }
}