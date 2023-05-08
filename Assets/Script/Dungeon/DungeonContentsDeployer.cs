using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using static UnityEditor.PlayerSettings;

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
    /// ダンジョン配置イベント
    /// </summary>
    IObservable<Unit> OnContentsInitialize { get; }
}

public class DungeonContentsDeployer : Singleton<DungeonContentsDeployer, IDungeonContentsDeployer>, IDungeonContentsDeployer
{
    /// <summary>
    /// イベント
    /// </summary>
    private Subject<Unit> m_OnContentsInitialize = new Subject<Unit>();
    IObservable<Unit> IDungeonContentsDeployer.OnContentsInitialize => m_OnContentsInitialize;

    /// <summary>
    /// 全て配置
    /// </summary>
    private void DeployAll()
    {
        Debug.Log("Deploy Contents");
        Deploy(CONTENTS_TYPE.PLAYER);
        Deploy(CONTENTS_TYPE.FRIEND);
        Deploy(CONTENTS_TYPE.ENEMY, DungeonProgressManager.Interface.CurrentDungeonSetup.EnemyCountMax);
        Deploy(CONTENTS_TYPE.ITEM, DungeonProgressManager.Interface.CurrentDungeonSetup.ItemCountMax);
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

        m_OnContentsInitialize.OnNext(Unit.Default);
    }

    /// <summary>
    /// ダンジョンコンテンツ全て撤去
    /// </summary>
    private void RemoveAll()
    {
        // ----- Player ----- //
        foreach (var player in UnitHolder.Interface.FriendList)
        {
            player.Dispose();
        }

        // ----- Enemy ----- //
        foreach (var enemy in UnitHolder.Interface.EnemyList)
        {
            string name = enemy.GetInterface<ICharaStatus>().Parameter.GivenName.ToString();
            ObjectPoolController.Interface.SetObject(name, enemy.GetInterface<ICharaObjectHolder>().MoveObject);
            enemy.Dispose();
        }
        UnitHolder.Interface.EnemyList.Clear();

        // ----- Item ----- //
        foreach (var item in ItemManager.Interface.ItemList)
        {
            string name = item.Setup.ItemName;
            ObjectPoolController.Interface.SetObject(name, item.ItemObject);
        }
        ItemManager.Interface.ItemList.Clear();
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
                    // 再配置
                    if (UnitHolder.Interface.FriendList.Count != 0)
                        RedeployPlayer();
                    else
                    {
                        // 配置位置決め
                        var cellPos = DungeonHandler.Interface.GetRandomRoomEmptyCellPosition(); //何もない部屋座標を取得
                        var pos = new Vector3(cellPos.x, CharaMove.OFFSET_Y, cellPos.z);
                        gameObject.transform.position = pos;

                        DeployPlayer(pos);
                    }
                    break;

                case CONTENTS_TYPE.FRIEND:
                    // 再配置
                    if (UnitHolder.Interface.FriendList.Count >= 2)
                        RedeployFriend();
                    else
                    {
                        var pos = PlayerAroundPos();
                        DeployFriend(pos);
                    }
                    break;

                case CONTENTS_TYPE.ENEMY:
                    DeployEnemy();
                    break;

                case CONTENTS_TYPE.ITEM:
                    DeployItem();
                    break;
            }
        }

        // プレイヤー再配置
        void RedeployPlayer()
        {
            // 使うもの
            var player = UnitHolder.Interface.Player;
            var gameObject = player.GetInterface<ICharaObjectHolder>().MoveObject;

            // 配置
            var cellPos = DungeonHandler.Interface.GetRandomRoomEmptyCellPosition(); //何もない部屋座標を取得
            var pos = new Vector3(cellPos.x, CharaMove.OFFSET_Y, cellPos.z);
            gameObject.transform.position = pos;

            // 初期化
            player.Initialize();
        }

        // プレイヤーの周囲マスを取得
        Vector3 PlayerAroundPos()
        {
            var playerPos = UnitHolder.Interface.Player.GetInterface<ICharaMove>().Position;
            var around = DungeonHandler.Interface.GetAroundCellId(playerPos);
            var dir = DIRECTION.NONE;
            foreach (var near in around.Cells)
            {
                if (near.Value == CELL_ID.ROOM)
                {
                    dir = near.Key;
                    break;
                }
            }
            var nearPos = playerPos + dir.ToV3Int() + new Vector3(0f, CharaMove.OFFSET_Y, 0f);
            return nearPos;
        }

        // バディ再配置
        void RedeployFriend()
        {
            // 使うもの
            var friend = UnitHolder.Interface.FriendList[1];
            var gameObject = friend.GetInterface<ICharaObjectHolder>().MoveObject;

            // 配置
            gameObject.transform.position = PlayerAroundPos();

            // 初期化
            friend.Initialize();
        }

        // 敵配置
        void DeployEnemy()
        {
            // 使うもの
            var setup = DungeonProgressManager.Interface.GetRandomEnemySetup();
            var gameObject = ObjectPoolController.Interface.GetObject(setup);

            // 配置
            var cellPos = DungeonHandler.Interface.GetRandomRoomEmptyCellPosition(); //何もない部屋座標を取得
            var pos = new Vector3(cellPos.x, CharaMove.OFFSET_Y, cellPos.z);
            gameObject.transform.position = pos;

            // 初期化
            var enemy = gameObject.GetComponent<ICollector>();
            if (enemy.RequireInterface<ICharaStatus>(out var e) == true)
                e.SetStatus(setup as CharacterSetup);
            enemy.Initialize();

            // 追加
            UnitHolder.Interface.AddEnemy(enemy);
        }

        // アイテム配置
        void DeployItem()
        {
            // 使うもの
            var setup = DungeonProgressManager.Interface.GetRandomItemSetup();
            var content = ObjectPoolController.Interface.GetObject(setup);

            // 初期化
            var cellPos = DungeonHandler.Interface.GetRandomRoomEmptyCellPosition(); //何もない部屋座標を取得
            var pos = new Vector3(cellPos.x, ItemHandler.OFFSET_Y, cellPos.z);
            IItemHandler item = content.GetComponent<ItemHandler>();
            item.Initialize(setup as ItemSetup, content, pos);

            // 追加
            ItemManager.Interface.AddItem(item);
        }
    }

    /// <summary>
    /// プレイヤー配置
    /// </summary>
    /// <param name="pos"></param>
    private void DeployPlayer(Vector3 pos)
    {
        // 使うもの
        var setup = OutGameInfoHolder.Interface.Leader;
        var gameObject = ObjectPoolController.Interface.GetObject(setup);

        // 初期化
        var player = gameObject.GetComponent<ICollector>();
        if (player.RequireInterface<ICharaStatus>(out var p) == true)
            p.SetStatus(setup as CharacterSetup);
        player.Initialize();

        // カメラセット
        CameraHandler.Interface.SetParent(gameObject);

        // 追加
        UnitHolder.Interface.AddPlayer(player);
    }

    /// <summary>
    /// バディ配置
    /// </summary>
    /// <param name="pos"></param>
    private void DeployFriend(Vector3 pos)
    {
        // 使うもの
        var setup = OutGameInfoHolder.Interface.Friend;
        var gameObject = ObjectPoolController.Interface.GetObject(setup);

        // 配置
        gameObject.transform.position = pos;

        // 初期化
        var friend = gameObject.GetComponent<ICollector>();
        if (friend.RequireInterface<ICharaStatus>(out var f) == true)
            f.SetStatus(setup as CharacterSetup);
        friend.Initialize();

        // 追加
        UnitHolder.Interface.AddPlayer(friend);
    }

    /// <summary>
    /// ボス配置
    /// </summary>
    /// <param name="pos"></param>
    private void DeployBoss(Vector3 pos, CharacterSetup setup)
    {
        var gameObject = ObjectPoolController.Interface.GetObject(setup);

        // 配置
        gameObject.transform.position = pos;

        // 初期化
        var boss = gameObject.GetComponent<ICollector>();
        if (boss.RequireInterface<ICharaStatus>(out var b) == true)
            b.SetStatus(setup as CharacterSetup);
        boss.Initialize();

        // 追加
        UnitHolder.Interface.AddEnemy(boss);
    }
}