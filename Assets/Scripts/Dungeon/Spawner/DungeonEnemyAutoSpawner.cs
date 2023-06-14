using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;
using Zenject;

public interface IDungeonEnemyAutoSpawner
{
    /// <summary>
    /// 敵の自動沸き
    /// </summary>
    bool AutoSpawnEnemy { set; }
}

public class DungeonEnemyAutoSpawner : IDungeonEnemyAutoSpawner, IInitializable
{
    [Inject]
    private IDungeonEnemySpawner m_EnemySpawner;
    [Inject]
    private IDungeonHandler m_DungeonHandler;
    [Inject]
    private IUnitHolder m_UnitHolder;
    [Inject]
    private ITurnManager m_TurnManager;
    [Inject]
    private IDungeonProgressManager m_DungeonProgressManager;
    [Inject]
    private DungeonProgressHolder m_DungeonProgressHolder;

    private static int SPAWN_INTERVAL = 30;
    private int m_SpawnTurnCount;

    void IInitializable.Initialize()
    {
        m_AutoSpawnEnemy = true;
        m_TurnManager.OnTurnEnd.SubscribeWithState(this, async (_, self) =>
        {
            if (self.m_UnitHolder.EnemyCount < self.m_DungeonProgressHolder.CurrentDungeonSetup.EnemyCountMax)
                self.m_SpawnTurnCount++;
            else
                self.m_SpawnTurnCount = 0;

            if (self.m_AutoSpawnEnemy == true && self.m_SpawnTurnCount >= SPAWN_INTERVAL)
            {
                self.m_SpawnTurnCount = 0;
                await self.SpawnRandomEnemy();
            }
        });
    }

    /// <summary>
    /// 敵の自動沸き
    /// </summary>
    private bool m_AutoSpawnEnemy;
    bool IDungeonEnemyAutoSpawner.AutoSpawnEnemy { set => m_AutoSpawnEnemy = value; }

    /// <summary>
    /// 敵をランダムスポーンさせる
    /// </summary>
    /// <param name="count"></param>
    private async Task SpawnRandomEnemy()
    {
        var playerPos = m_UnitHolder.Player.GetInterface<ICharaMove>().Position;
        if (m_DungeonHandler.TryGetRoomId(playerPos, out var id) == false)
        {
            await m_EnemySpawner.SpawnRandomEnemy(1);
            return;
        }

        // 敵のセットアップをランダム取得
        var setup = m_DungeonProgressManager.GetRandomEnemySetup();
        Vector3 pos = default;

        while (true)
        {
            // 座標
            var cellPos = m_DungeonHandler.GetRandomRoomEmptyCellPosition(); //何もない部屋座標を取得
            if (m_DungeonHandler.TryGetRoomId(cellPos, out var spawnId) == false)
                continue;

            // 違う部屋なら
            if (id != spawnId)
            {
                pos = new Vector3(cellPos.x, CharaMove.OFFSET_Y, cellPos.z);
                break;
            }
        }

        await m_EnemySpawner.SpawnEnemy(setup, pos);
    }
}
