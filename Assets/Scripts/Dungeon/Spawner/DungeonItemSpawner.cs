using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public interface IDungeonItemSpawner
{
    /// <summary>
    /// アイテムを生成する
    /// </summary>
    /// <param name="setup"></param>
    /// <returns></returns>
    Task SpawnItem(ItemSetup setup, Vector3Int pos);

    /// <summary>
    /// アイテムを生成する
    /// </summary>
    /// <param name="setup"></param>
    /// <returns></returns>
    Task SpawnItem(ItemSetup setup, Vector3Int pos, GameObject content);

    /// <summary>
    /// ランダムなアイテムを生成する
    /// </summary>
    /// <param name="count"></param>
    /// <returns></returns>
    Task SpawnRandomItem(int count);
}

public class DungeonItemSpawner : IDungeonItemSpawner
{
    [Inject]
    private IDungeonProgressManager m_DungeonProgressManager;
    [Inject]
    private IObjectPoolController m_ObjectPoolController;
    [Inject]
    private IDungeonHandler m_DungeonHandler;
    [Inject]
    private IItemManager m_ItemManager;

    /// <summary>
    /// 任意のアイテムを生成する
    /// </summary>
    /// <param name="setup"></param>
    /// <returns></returns>
    private async Task SpawnItem(ItemSetup setup, Vector3Int pos)
    {
        var content = m_ObjectPoolController.GetObject(setup);
        await SpawnItem(setup, pos, content);
    }
    Task IDungeonItemSpawner.SpawnItem(ItemSetup setup, Vector3Int pos) => SpawnItem(setup, pos);

    /// <summary>
    /// 任意のアイテムを生成する
    /// </summary>
    /// <param name="setup"></param>
    /// <returns></returns>
    private Task SpawnItem(ItemSetup setup, Vector3Int pos, GameObject content)
    {
        ICollector item = content.GetComponent<ActorComponentCollector>();
        IItemHandler itemHandler = item.GetInterface<IItemHandler>();
        itemHandler.SetInfo(setup, content, pos);
        item.Initialize();

        // 追加
        m_ItemManager.AddItem(item);

        return Task.CompletedTask;
    }
    Task IDungeonItemSpawner.SpawnItem(ItemSetup setup, Vector3Int pos, GameObject content) => SpawnItem(setup, pos, content);

    /// <summary>
    /// アイテムをランダムスポーンさせる
    /// </summary>
    /// <param name="count"></param>
    async Task IDungeonItemSpawner.SpawnRandomItem(int count)
    {
        for (int i = 0; i < count; i++)
        {
            // 使うもの
            var setup = m_DungeonProgressManager.GetRandomItemSetup();
            // 初期化
            var cellPos = m_DungeonHandler.GetRandomRoomEmptyCellPosition(); //何もない部屋座標を取得
            var pos = new Vector3Int(cellPos.x, 0, cellPos.z);
            await SpawnItem(setup, pos);
        }
    }
}
