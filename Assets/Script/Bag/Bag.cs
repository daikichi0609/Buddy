using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bag 
{
    public List<Item> ItemList
    {
        get;
    } = new List<Item>();

    public int InventoryCount
    {
        get => BagManager.Instance.InventryCount;
    }

    public bool PutAway(GameObject obj)
    {
        Item item = obj.GetComponent<Item>();
        if(item == null)
        {
            Debug.Log("不正なアイテムです");
            return false;
        }

        if(ItemList.Count < InventoryCount)
        {
            ItemList.Add(item);
            ObjectPool.Instance.SetObject(item.Name.ToString(), obj);
            return true;
        }
        else
        {
            Debug.Log("アイテムがいっぱいです");
            return false;
        }
    }
}
