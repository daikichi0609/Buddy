using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bag 
{
    private static readonly int InventoryCount = 9;

    public List<IItem> ItemList { get; } = new List<IItem>();

    public bool PutAway(IItem item)
    {
        if(ItemList.Count < InventoryCount)
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
