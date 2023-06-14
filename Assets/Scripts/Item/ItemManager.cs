using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using System.Linq;
using Zenject;
using System.Threading.Tasks;
using DG.Tweening;

public interface IItemManager
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

    /// <summary>
    /// アイテムを飛ばす
    /// </summary>
    /// <param name="setup"></param>
    /// <returns></returns>
    Task FlyItem(ItemSetup setup, Vector3Int from, Vector3Int to, bool isDrop);
}

public class ItemManager : IItemManager
{
    [Inject]
    private IObjectPoolController m_ObjectPoolContoroller;
    [Inject]
    private IDungeonItemSpawner m_ItemSpawner;

    [Inject]
    public void Construct(IDungeonContentsDeployer dungeonContentsDeployer)
    {
        dungeonContentsDeployer.OnRemoveContents.SubscribeWithState(this, (_, self) => self.m_ItemList.Clear());
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

    /// <summary>
    /// アイテムを飛ばす
    /// </summary>
    /// <param name="setup"></param>
    /// <returns></returns>
    async Task IItemManager.FlyItem(ItemSetup setup, Vector3Int from, Vector3Int dir, bool isDrop)
    {
        var content = m_ObjectPoolContoroller.GetObject(setup);
        content.transform.position = from + new Vector3(0f, ItemHandler.OFFSET_Y, 0f);
        await content.transform.DOLocalMove(dir, 0.5f).SetRelative(true).SetEase(Ease.Linear).AsyncWaitForCompletion();

        if (isDrop == true)
        {
            var destPos = from + dir;
            await m_ItemSpawner.SpawnItem(setup, destPos, content);
        }
        else
        {
            m_ObjectPoolContoroller.SetObject(setup, content);
        }
    }
}
