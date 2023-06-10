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
    IItemHandler[] Items { get; }

    /// <summary>
    /// アイテムをしまう
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    bool TryPut(IItemHandler item);

    /// <summary>
    /// アイテムを消費する
    /// </summary>
    /// <param name="item"></param>
    void Consume(IItemHandler item);
}

public class TeamInventory : ITeamInventory
{
    private static readonly int InventoryCount = 9;

    private List<IItemHandler> m_ItemList = new List<IItemHandler>();
    IItemHandler[] ITeamInventory.Items => m_ItemList.ToArray();

    /// <summary>
    /// アイテムをしまう
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    bool ITeamInventory.TryPut(IItemHandler item)
    {
        if (m_ItemList.Count < InventoryCount)
        {
            m_ItemList.Add(item);
            item.OnPut();
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
    void ITeamInventory.Consume(IItemHandler item)
    {
        m_ItemList.Remove(item);
    }
}
