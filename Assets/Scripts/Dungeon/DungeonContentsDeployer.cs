using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using Zenject;
using System.Threading.Tasks;
using Fungus;
using Task = System.Threading.Tasks.Task;

public interface IDungeonContentsDeployer
{
    /// <summary>
    /// コンテンツ全部配置
    /// </summary>
    Task DeployAll();

    /// <summary>
    /// ボスバトル用
    /// </summary>
    /// <param name="playerPos"></param>
    /// <param name="friendPos"></param>
    /// <param name="bossPos"></param>
    Task DeployBossBattleContents(BossBattleDeployInfo info);

    /// <summary>
    /// ボスバトル用（キング）
    /// </summary>
    /// <param name="playerPos"></param>
    /// <param name="friendPos"></param>
    /// <param name="bossPos"></param>
    Task DeployBossBattleContents(KingBattleDeployInfo info);

    /// <summary>
    /// コンテンツ撤去
    /// </summary>
    void RemoveAll();

    /// <summary>
    /// コンテンツ配置イベント
    /// </summary>
    IObservable<Unit> OnDeployContents { get; }

    /// <summary>
    /// コンテンツ撤去イベント
    /// </summary>
    IObservable<Unit> OnRemoveContents { get; }

    /// <summary>
    /// 敵召喚スキル用
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    Task DeployEnemy(CharacterSetup setup, Vector3 pos);
}

public class DungeonContentsDeployer : IDungeonContentsDeployer
{
    [Inject]
    private IDungeonHandler m_DungeonHandler;
    [Inject]
    private IUnitHolder m_UnitHolder;
    [Inject]
    private IItemManager m_ItemManager;
    [Inject]
    private InGameProgressHolder m_InGameProgressHolder;
    [Inject]
    private DungeonProgressHolder m_DungeonProgressHolder;
    [Inject]
    private CurrentCharacterHolder m_CurrentCharacterHolder;
    [Inject]
    private IDungeonFriendSpawner m_FriendSpawner;
    [Inject]
    private IDungeonEnemySpawner m_EnemySpawner;
    [Inject]
    private IDungeonItemSpawner m_ItemSpawner;

    /// <summary>
    /// デプロイイベント
    /// </summary>
    private Subject<Unit> m_OnDeployContents = new Subject<Unit>();
    IObservable<Unit> IDungeonContentsDeployer.OnDeployContents => m_OnDeployContents;

    /// <summary>
    /// 撤去イベント
    /// </summary>
    private Subject<Unit> m_RemoveContents = new Subject<Unit>();
    IObservable<Unit> IDungeonContentsDeployer.OnRemoveContents => m_RemoveContents;

    /// <summary>
    /// 全て配置
    /// </summary>
    private async Task DeployAll()
    {
        await DeployLeader();
        await DeployFriend();
        await m_EnemySpawner.SpawnRandomEnemy(m_DungeonProgressHolder.CurrentDungeonSetup.EnemyCountMax);
        await m_ItemSpawner.SpawnRandomItem(m_DungeonProgressHolder.CurrentDungeonSetup.ItemCountMax);

        m_OnDeployContents.OnNext(Unit.Default);
    }
    async Task IDungeonContentsDeployer.DeployAll() => await DeployAll();

    /// <summary>
    /// ボス戦に必要なもの配置
    /// </summary>
    async Task IDungeonContentsDeployer.DeployBossBattleContents(BossBattleDeployInfo info)
    {
        await DeployLeader(info.PlayerPos, DIRECTION.UP);
        await DeployFriend(info.FriendPos, DIRECTION.UP);
        await m_EnemySpawner.SpawnEnemy(info.BossCharacterSetup, info.BossPos);

        m_OnDeployContents.OnNext(Unit.Default);
    }

    /// <summary>
    /// キング戦
    /// </summary>
    async Task IDungeonContentsDeployer.DeployBossBattleContents(KingBattleDeployInfo info)
    {
        await DeployLeader(info.PlayerPos, DIRECTION.UP);
        await m_EnemySpawner.SpawnEnemy(info.BossCharacterSetup, info.BossPos);
        for (int i = 0; i < info.WarriorPos.Length; i++)
            await m_EnemySpawner.SpawnEnemy(info.WarriorSetup, info.WarriorPos[i], info.WarriorDir[i]);

        if (info.FriendPos != null && info.FriendDir != null)
            for (int j = 0; j < info.FriendPos.Length; j++)
                await m_FriendSpawner.SpawnFriendNotDeadEnd(info.FriendSetup[j], info.FriendPos[j], info.FriendDir[j]);

        m_OnDeployContents.OnNext(Unit.Default);
    }

    /// <summary>
    /// ダンジョンコンテンツ全て撤去
    /// </summary>
    private void RemoveAll()
    {
        // ----- Player ----- //
        foreach (var player in m_UnitHolder.FriendList)
            player.Dispose();

        // ----- Enemy ----- //
        foreach (var enemy in m_UnitHolder.EnemyList)
            enemy.Dispose();

        // ----- Item ----- //
        foreach (var item in m_ItemManager.ItemList)
            item.Dispose();

        m_RemoveContents.OnNext(Unit.Default);
    }
    void IDungeonContentsDeployer.RemoveAll() => RemoveAll();

    /// <summary>
    /// リーダー配置
    /// </summary>
    /// <param name="pos"></param>
    private async Task DeployLeader()
    {
        var cellPos = m_DungeonHandler.GetRandomRoomEmptyCellPosition(); //何もない部屋座標を取得
        var pos = new Vector3(cellPos.x, CharaMove.OFFSET_Y, cellPos.z); // 配置位置
        await DeployLeader(pos);
    }
    private async Task DeployLeader(Vector3 pos, DIRECTION dir = DIRECTION.UNDER)
    {
        var setup = m_CurrentCharacterHolder.Leader; // Setup
        await m_FriendSpawner.SpawnLeader(setup, pos, dir);
    }

    /// <summary>
    /// バディ配置
    /// </summary>
    /// <param name="pos"></param>
    private async Task DeployFriend()
    {
        // 配置位置決め
        var nearPos = PlayerAroundPos();
        await DeployFriend(nearPos);

        // プレイヤーの周囲マスを取得
        Vector3 PlayerAroundPos()
        {
            var playerPos = m_UnitHolder.Player.GetInterface<ICharaMove>().Position;
            var around = m_DungeonHandler.GetAroundCellId(playerPos);
            var dir = DIRECTION.NONE;
            foreach (var near in around.AroundCells)
            {
                if (near.Value == TERRAIN_ID.ROOM && near.Key.IsDiagonal() == false)
                {
                    dir = near.Key;
                    break;
                }
            }
#if DEBUG
            if (dir == DIRECTION.NONE)
                Debug.Log("プレイヤーの近くのセルを取得できません");
#endif
            var nearPos = playerPos + dir.ToV3Int() + new Vector3(0f, CharaMove.OFFSET_Y, 0f);
            return nearPos;
        }
    }
    private async Task DeployFriend(Vector3 pos, DIRECTION dir = DIRECTION.UNDER)
    {
        if (m_CurrentCharacterHolder.TryGetFriend(m_InGameProgressHolder.InGameProgress, out var setup) == true)
            await m_FriendSpawner.SpawnFriend(setup, pos, dir);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    async Task IDungeonContentsDeployer.DeployEnemy(CharacterSetup setup, Vector3 pos) => await m_EnemySpawner.SpawnEnemy(setup, pos);
}