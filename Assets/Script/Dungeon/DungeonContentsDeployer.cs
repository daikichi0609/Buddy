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
    void Deploy();

    /// <summary>
    /// コンテンツ撤去
    /// </summary>
    void Remove();

    /// <summary>
    /// ダンジョン配置イベント
    /// </summary>
    IObservable<Unit> OnContentsInitialize { get; }
}

public class DungeonContentsDeployer : Singleton<DungeonContentsDeployer, IDungeonContentsDeployer>, IDungeonContentsDeployer
{
    protected override void Awake()
    {
        base.Awake();
        PlayerLoopManager.Interface.GetInitEvent.Subscribe(_ => DeployAll()).AddTo(this);
    }

    /// <summary>
    /// 
    /// </summary>
    private Subject<Unit> m_OnContentsInitialize = new Subject<Unit>();
    IObservable<Unit> IDungeonContentsDeployer.OnContentsInitialize => m_OnContentsInitialize;

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
                        DeployPlayer();
                    break;

                case CONTENTS_TYPE.FRIEND:
                    // 再配置
                    if (UnitHolder.Interface.FriendList.Count >= 2)
                        RedeployFriend();
                    else
                        DeployFriend();
                    break;

                case CONTENTS_TYPE.ENEMY:
                    DeployEnemy();
                    break;

                case CONTENTS_TYPE.ITEM:
                    DeployItem();
                    break;
            }
        }

        //　プレイヤー配置
        void DeployPlayer()
        {
            // 使うもの
            var setup = OutGameInfoHolder.Interface.Leader;
            var gameObject = ObjectPoolController.Interface.GetObject(setup);

            // 配置位置決め
            var cell = DungeonHandler.Interface.GetRandomRoomEmptyCell(); //何もない部屋座標を取得
            var info = cell.GetInterface<ICellInfoHolder>();
            var pos = new Vector3(info.X, CharaMove.OFFSET_Y, info.Z);
            gameObject.transform.position = pos;

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

        // プレイヤー再配置
        void RedeployPlayer()
        {
            // 使うもの
            var player = UnitHolder.Interface.Player;
            var gameObject = player.GetInterface<ICharaObjectHolder>().MoveObject;

            // 配置
            var cell = DungeonHandler.Interface.GetRandomRoomEmptyCell(); //何もない部屋座標を取得
            var info = cell.GetInterface<ICellInfoHolder>();
            var pos = new Vector3(info.X, CharaMove.OFFSET_Y, info.Z);
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

        // バディ配置
        void DeployFriend()
        {
            // 使うもの
            var setup = OutGameInfoHolder.Interface.Friend;
            var gameObject = ObjectPoolController.Interface.GetObject(setup);

            // 配置
            gameObject.transform.position = PlayerAroundPos();

            // 初期化
            var friend = gameObject.GetComponent<ICollector>();
            if (friend.RequireInterface<ICharaStatus>(out var f) == true)
                f.SetStatus(setup as CharacterSetup);
            friend.Initialize();

            // 追加
            UnitHolder.Interface.AddPlayer(friend);
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
            var cell = DungeonHandler.Interface.GetRandomRoomEmptyCell(); //何もない部屋座標を取得
            var info = cell.GetInterface<ICellInfoHolder>();
            var pos = new Vector3(info.X, CharaMove.OFFSET_Y, info.Z);
            gameObject.transform.position = new Vector3(info.X, CharaMove.OFFSET_Y, info.Z);

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
            var cell = DungeonHandler.Interface.GetRandomRoomEmptyCell(); //何もない部屋座標を取得
            var info = cell.GetInterface<ICellInfoHolder>();
            var pos = new Vector3(info.X, ItemHandler.OFFSET_Y, info.Z);
            IItemHandler item = content.GetComponent<ItemHandler>();
            item.Initialize(setup as ItemSetup, content, pos);

            // 追加
            ItemManager.Interface.AddItem(item);
        }
    }

    private void DeployAll()
    {
        Debug.Log("Deploy Contents");
        Deploy(CONTENTS_TYPE.PLAYER);
        Deploy(CONTENTS_TYPE.FRIEND);
        Deploy(CONTENTS_TYPE.ENEMY, DungeonProgressManager.Interface.CurrentDungeonSetup.EnemyCountMax);
        Deploy(CONTENTS_TYPE.ITEM, DungeonProgressManager.Interface.CurrentDungeonSetup.ItemCountMax);

        m_OnContentsInitialize.OnNext(Unit.Default);
    }
    void IDungeonContentsDeployer.Deploy() => DeployAll();

    //ダンジョンコンテンツ撤去
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

    void IDungeonContentsDeployer.Remove()
    {
        RemoveAll();
    }
}