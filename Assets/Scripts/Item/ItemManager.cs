using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using System.Linq;
using Zenject;

public interface IItemManager : ISingleton
{
    /// <summary>
    /// 全てのアイテム
    /// </summary>
    List<IItemHandler> ItemList { get; }

    /// <summary>
    /// アイテム追加
    /// </summary>
    /// <param name="item"></param>
    void AddItem(IItemHandler item);

    /// <summary>
    /// アイテム削除
    /// </summary>
    /// <param name="item"></param>
    void RemoveItem(IItemHandler item);

    /// <summary>
    /// アイテムがあるか
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    bool IsItemOn(Vector3Int pos);
}

public class ItemManager : IItemManager
{
    [Inject]
    public void Construct(IDungeonContentsDeployer dungeonContentsDeployer)
    {
        dungeonContentsDeployer.OnRemoveContents.Subscribe(_ => m_ItemList.Clear());
    }

    /// <summary>
    /// 落ちているアイテムリスト
    /// </summary>
    private List<IItemHandler> m_ItemList = new List<IItemHandler>();
    List<IItemHandler> IItemManager.ItemList => m_ItemList;

    void IItemManager.AddItem(IItemHandler item) => m_ItemList.Add(item);
    void IItemManager.RemoveItem(IItemHandler item) => m_ItemList.Remove(item);

    /// <summary>
    /// アイテムがあるか
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    bool IItemManager.IsItemOn(Vector3Int pos)
    {
        foreach (var item in m_ItemList)
        {
            if (item.Position == pos)
                return true;
        }
        return false;
    }
}
