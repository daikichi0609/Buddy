using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public interface ITeamInventory
{
    /// <summary>
    /// 所持しているアイテム
    /// </summary>
    ItemSetup[] Items { get; }

    /// <summary>
    /// アイテムセット
    /// </summary>
    /// <param name="items"></param>
    void SetItems(ItemSetup[] items);

    /// <summary>
    /// アイテムをしまう
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    bool TryPut(ItemSetup item);

    /// <summary>
    /// アイテムを消費する
    /// </summary>
    /// <param name="item"></param>
    void Consume(ItemSetup item);
}

public class TeamInventory : ITeamInventory
{
    private static readonly int InventoryCount = 9;

    private List<ItemSetup> m_ItemList = new List<ItemSetup>();
    ItemSetup[] ITeamInventory.Items => m_ItemList.ToArray();

    void ITeamInventory.SetItems(ItemSetup[] items)
    {
        m_ItemList.Clear();

        foreach (var item in items)
            m_ItemList.Add(item);
    }

    /// <summary>
    /// アイテムをしまう
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    bool ITeamInventory.TryPut(ItemSetup item)
    {
        if (m_ItemList.Count < InventoryCount)
        {
            m_ItemList.Add(item);
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
    void ITeamInventory.Consume(ItemSetup item)
    {
        m_ItemList.Remove(item);
    }
}
