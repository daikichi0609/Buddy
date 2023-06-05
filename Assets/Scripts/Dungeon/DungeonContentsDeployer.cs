using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using Zenject;

public enum CONTENTS_TYPE
{
    PLAYER,
    FRIEND,
    ENEMY,
    ITEM,
}

public interface IDungeonContentsDeployer : ISingleton
{
    /// <summary>
    /// コンテンツ全部配置
    /// </summary>
    void DeployAll();

    /// <summary>
    /// ボスバトル用
    /// </summary>
    /// <param name="playerPos"></param>
    /// <param name="friendPos"></param>
    /// <param name="bossPos"></param>
    void DeployBossBattleContents(BossBattleDeployInfo info);

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
}

public class DungeonContentsDeployer : IDungeonContentsDeployer
{
    [Inject]
    private IDungeonHandler m_DungeonHandler;
    [Inject]
    private IObjectPoolController m_ObjectPoolController;
    [Inject]
    private IItemManager m_ItemManager;
    [Inject]
    private IUnitHolder m_UnitHolder;
    [Inject]
    protected IDungeonProgressManager m_DungeonProgressManager;
    [Inject]
    protected DungeonProgressHolder m_DungeonProgressHolder;
    [Inject]
    protected OutGameInfoHolder m_OutGameInfoHolder;

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
    private void DeployAll()
    {
        Debug.Log("Deploy Contents");
        Deploy(CONTENTS_TYPE.PLAYER);
        Deploy(CONTENTS_TYPE.FRIEND);
        Deploy(CONTENTS_TYPE.ENEMY, m_DungeonProgressHolder.CurrentDungeonSetup.EnemyCountMax);
        Deploy(CONTENTS_TYPE.ITEM, m_DungeonProgressHolder.CurrentDungeonSetup.ItemCountMax);

        m_OnDeployContents.OnNext(Unit.Default);
    }
    void IDungeonContentsDeployer.DeployAll() => DeployAll();

    /// <summary>
    /// ボス戦に必要なもの配置
    /// </summary>
    /// <param name="playerPos"></param>
    /// <param name="friendPos"></param>
    /// <param name="bossPos"></param>
    void IDungeonContentsDeployer.DeployBossBattleContents(BossBattleDeployInfo info)
    {
        DeployPlayer(info.PlayerPos);
        DeployFriend(info.FriendPos);
        DeployBoss(info.BossPos, info.BossCharacterSetup);

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
    /// コンテンツ配置
    /// </summary>
    /// <param name="type"></param>
    /// <param name="count"></param>
    private void Deploy(CONTENTS_TYPE type, int count = 1)
    {
        for (int i = 0; i < count; i++)
        {
            // ゲームオブジェクト取得
            switch (type)
            {
                case CONTENTS_TYPE.PLAYER:
                    // 配置位置決め
                    var cellPos = m_DungeonHandler.GetRandomRoomEmptyCellPosition(); //何もない部屋座標を取得
                    var pos = new Vector3(cellPos.x, CharaMove.OFFSET_Y, cellPos.z);
                    DeployPlayer(pos);
                    break;

                case CONTENTS_TYPE.FRIEND:
                    // 配置位置決め
                    var nearPos = PlayerAroundPos();
                    DeployFriend(nearPos);
                    break;

                case CONTENTS_TYPE.ENEMY:
                    DeployEnemy();
                    break;

                case CONTENTS_TYPE.ITEM:
                    DeployItem();
                    break;
            }
        }

        // プレイヤーの周囲マスを取得
        Vector3 PlayerAroundPos()
        {
            var playerPos = m_UnitHolder.Player.GetInterface<ICharaMove>().Position;
            var around = m_DungeonHandler.GetAroundCellId(playerPos);
            var dir = DIRECTION.NONE;
            foreach (var near in around.Cells)
            {
                if (near.Value == TERRAIN_ID.ROOM)
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

        // 敵配置
        void DeployEnemy()
        {
            // 使うもの
            var setup = m_DungeonProgressManager.GetRandomEnemySetup();
            var gameObject = m_ObjectPoolController.GetObject(setup);

            // 配置
            var cellPos = m_DungeonHandler.GetRandomRoomEmptyCellPosition(); //何もない部屋座標を取得
            var pos = new Vector3(cellPos.x, CharaMove.OFFSET_Y, cellPos.z);
            gameObject.transform.position = pos;

            // 初期化
            var enemy = gameObject.GetComponent<ICollector>();
            if (enemy.RequireInterface<ICharaStatus>(out var e) == true)
                e.SetStatus(setup as CharacterSetup);
            enemy.Initialize();

            // 追加
            m_UnitHolder.AddEnemy(enemy);
        }

        // アイテム配置
        void DeployItem()
        {
            // 使うもの
            var setup = m_DungeonProgressManager.GetRandomItemSetup();
            var content = m_ObjectPoolController.GetObject(setup);

            // 初期化
            var cellPos = m_DungeonHandler.GetRandomRoomEmptyCellPosition(); //何もない部屋座標を取得
            var pos = new Vector3Int(cellPos.x, 0, cellPos.z);
            IItemHandler item = content.GetComponent<ItemHandler>();
            item.Initialize(setup as ItemSetup, content, pos);

            // 追加
            m_ItemManager.AddItem(item);
        }
    }

    /// <summary>
    /// プレイヤー配置
    /// </summary>
    /// <param name="pos"></param>
    private void DeployPlayer(Vector3 pos)
    {
        // 使うもの
        var setup = m_OutGameInfoHolder.Leader;
        var playerObject = m_ObjectPoolController.GetObject(setup);

        // 配置
        playerObject.transform.position = pos;

        // 初期化
        var player = playerObject.GetComponent<ICollector>();
        if (player.RequireInterface<ICharaStatus>(out var p) == true)
            if (p.Setup == null)
                p.SetStatus(setup as CharacterSetup);

        player.Initialize();

        // 追加
        m_UnitHolder.AddPlayer(player);
    }

    /// <summary>
    /// バディ配置
    /// </summary>
    /// <param name="pos"></param>
    private void DeployFriend(Vector3 pos)
    {
        // 使うもの
        var setup = m_OutGameInfoHolder.Friend;
        var friendObject = m_ObjectPoolController.GetObject(setup);

        // 配置
        friendObject.transform.position = pos;

        // 初期化
        var friend = friendObject.GetComponent<ICollector>();
        if (friend.RequireInterface<ICharaStatus>(out var f) == true)
            if (f.Setup == null)
                f.SetStatus(setup as CharacterSetup);

        friend.Initialize();

        // 追加
        m_UnitHolder.AddPlayer(friend);
    }

    /// <summary>
    /// ボス配置
    /// </summary>
    /// <param name="pos"></param>
    private void DeployBoss(Vector3 pos, CharacterSetup setup)
    {
        var gameObject = m_ObjectPoolController.GetObject(setup);

        // 配置
        gameObject.transform.position = pos;

        // 初期化
        var boss = gameObject.GetComponent<ICollector>();
        if (boss.RequireInterface<ICharaStatus>(out var b) == true)
            b.SetStatus(setup as CharacterSetup);
        boss.Initialize();

        // 追加
        m_UnitHolder.AddEnemy(boss);
    }
}