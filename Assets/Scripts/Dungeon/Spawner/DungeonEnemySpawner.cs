using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;
using Zenject;

public interface IDungeonEnemySpawner
{
    /// <summary>
    /// 敵を生成する
    /// </summary>
    /// <param name="setup"></param>
    /// <returns></returns>
    Task SpawnEnemy(CharacterSetup setup, Vector3 pos, DIRECTION dir = DIRECTION.UNDER);

    /// <summary>
    /// ランダムな敵を生成する
    /// </summary>
    /// <param name="count"></param>
    /// <returns></returns>
    Task SpawnRandomEnemy(int count);
}

public class DungeonEnemySpawner : IDungeonEnemySpawner
{
    [Inject]
    private DungeonProgressHolder m_DungeonProgressHolder;
    [Inject]
    private IObjectPoolController m_ObjectPoolController;
    [Inject]
    private IDungeonHandler m_DungeonHandler;
    [Inject]
    private IUnitHolder m_UnitHolder;

    private static IInjector ms_EnemyInjector = new EnemyInjector();

    /// <summary>
    /// 任意の敵を生成する
    /// </summary>
    /// <param name="setup"></param>
    /// <returns></returns>
    private async Task SpawnEnemy(CharacterSetup setup, Vector3 pos, DIRECTION dir = DIRECTION.UNDER)
    {
        var gameObject = m_ObjectPoolController.GetObject(setup, ms_EnemyInjector);
        gameObject.transform.position = pos;

        // 初期化
        var enemy = gameObject.GetComponent<ICollector>();
        if (enemy.RequireInterface<ICharaStatus>(out var e) == true)
            e.SetStatus(setup as CharacterSetup);
        enemy.Initialize();

        await enemy.GetInterface<ICharaMove>().Face(dir);

        // 追加
        m_UnitHolder.AddEnemy(enemy);
    }
    Task IDungeonEnemySpawner.SpawnEnemy(CharacterSetup setup, Vector3 pos, DIRECTION dir) => SpawnEnemy(setup, pos, dir);

    /// <summary>
    /// 敵をランダムスポーンさせる
    /// </summary>
    /// <param name="count"></param>
    private async Task SpawnRandomEnemy(int count)
    {
        for (int i = 0; i < count; i++)
        {
            // 敵のセットアップをランダム取得
            var setup = m_DungeonProgressHolder.GetRandomEnemySetup();

            // 座標
            var cellPos = m_DungeonHandler.GetRandomRoomEmptyCellPosition(); //何もない部屋座標を取得
            var pos = new Vector3(cellPos.x, CharaMove.OFFSET_Y, cellPos.z);

            await SpawnEnemy(setup, pos);
        }
    }
    Task IDungeonEnemySpawner.SpawnRandomEnemy(int count) => SpawnRandomEnemy(count);

    /// <summary>
    /// 敵をランダムスポーンさせる
    /// </summary>
    /// <param name="count"></param>
    private async Task SpawnRandomEnemy()
    {
        var playerPos = m_UnitHolder.Player.GetInterface<ICharaMove>().Position;
        if (m_DungeonHandler.TryGetRoomId(playerPos, out var id) == false)
        {
            await SpawnRandomEnemy(1);
            return;
        }

        // 敵のセットアップをランダム取得
        var setup = m_DungeonProgressHolder.GetRandomEnemySetup();
        Vector3 pos = default;

        while (true)
        {
            // 座標
            var cellPos = m_DungeonHandler.GetRandomRoomEmptyCellPosition(); //何もない部屋座標を取得
            if (m_DungeonHandler.TryGetRoomId(playerPos, out var spawnId) == false)
                continue;

            // 違う部屋なら
            if (id != spawnId)
            {
                pos = new Vector3(cellPos.x, CharaMove.OFFSET_Y, cellPos.z);
                break;
            }
        }

        await SpawnEnemy(setup, pos);
    }
}
