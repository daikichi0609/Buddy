using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;

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
    /// キャラオブジェクト取得
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private GameObject CharaObject(CharacterSetup setup)
    {
        if (ObjectPool.Instance.TryGetPoolObject(setup.name, out var chara) == false)
            chara = Instantiate(setup.Prefab);

        return chara;
    }

    /// <summary>
    /// コンテンツ配置
    /// </summary>
    /// <param name="type"></param>
    /// <param name="count"></param>
    private void Deploy(CONTENTS_TYPE type, int count = 1)
    {
        for (int i = 0; i < count; i++)
        {
            var cell = DungeonHandler.Interface.GetRandomRoomEmptyCell(); //何もない部屋座標を取得
            var info = cell.GetInterface<ICellInfoHolder>();
            GameObject content = null;

            // ゲームオブジェクト取得
            switch (type)
            {
                case CONTENTS_TYPE.PLAYER:
                    if (UnitHolder.Interface.FriendList.Count != 0)
                    {
                        RedeployPlayer(cell);
                        break;
                    }
                    content = CharaObject(OutGameInfoHolder.Interface.Leader);
                    content.transform.position = new Vector3(info.X, CharaMove.OFFSET_Y, info.Z);
                    var player = content.GetComponent<ICollector>();
                    player.Initialize();
                    if (player.RequireInterface<ICharaStatus>(out var p) == true)
                        p.SetStatus(OutGameInfoHolder.Interface.Leader.Status);
                    CameraHandler.Interface.SetParent(content);
                    UnitHolder.Interface.AddPlayer(player);
                    break;

                case CONTENTS_TYPE.FRIEND:
                    if (UnitHolder.Interface.FriendList.Count >= 2)
                    {
                        RedeployFriend(cell);
                        return;
                    }
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
                    var pos = playerPos + dir.ToV3Int() + new Vector3(0f, CharaMove.OFFSET_Y, 0f);
                    content = CharaObject(OutGameInfoHolder.Interface.Friend);
                    content.transform.position = pos;
                    var friend = content.GetComponent<ICollector>();
                    friend.Initialize();
                    if (friend.RequireInterface<ICharaStatus>(out var f) == true)
                        f.SetStatus(OutGameInfoHolder.Interface.Friend.Status);
                    UnitHolder.Interface.AddPlayer(friend);
                    break;

                case CONTENTS_TYPE.ENEMY:
                    var enemySetup = DungeonProgressManager.Interface.GetRandomEnemySetup();
                    content = CharaObject(enemySetup);
                    content.transform.position = new Vector3(info.X, CharaMove.OFFSET_Y, info.Z);
                    var enemy = content.GetComponent<ICollector>();
                    enemy.Initialize();
                    if (enemy.RequireInterface<ICharaStatus>(out var e) == true)
                        e.SetStatus(enemySetup.Status);
                    UnitHolder.Interface.AddEnemy(enemy);
                    break;

                case CONTENTS_TYPE.ITEM:
                    var itemSetup = DungeonProgressManager.Interface.GetRandomItemSetup();
                    content = itemSetup.Prefab;
                    content.transform.position = new Vector3(info.X, Item.OFFSET_Y, info.Z);
                    content.transform.eulerAngles = new Vector3(45f, 0f, 0f);
                    IItem item = content.GetComponent<Item>();
                    item.Position = info.Position;
                    ItemManager.Interface.AddItem(item);
                    break;
            }
        }
    }

    private void RedeployPlayer(ICollector cell)
    {
        var player = UnitHolder.Interface.Player;
        var content = player.GetInterface<ICharaObjectHolder>().MoveObject;
        var info = cell.GetInterface<ICellInfoHolder>();
        content.transform.position = new Vector3(info.X, CharaMove.OFFSET_Y, info.Z);
        player.Initialize();
    }

    private void RedeployFriend(ICollector cell)
    {
        var friend = UnitHolder.Interface.FriendList[1];
        var content = friend.GetInterface<ICharaObjectHolder>().MoveObject;
        var info = cell.GetInterface<ICellInfoHolder>();
        content.transform.position = new Vector3(info.X, CharaMove.OFFSET_Y, info.Z);
        friend.Initialize();
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
            ObjectPool.Instance.SetObject(name, enemy.GetInterface<ICharaObjectHolder>().MoveObject);
            enemy.Dispose();
        }
        UnitHolder.Interface.EnemyList.Clear();

        // ----- Item ----- //
        foreach (var item in ItemManager.Interface.ItemList)
        {
            string name = item.Name.ToString();
            ObjectPool.Instance.SetObject(name, item.ItemObject);
        }
        ItemManager.Interface.ItemList.Clear();
    }

    void IDungeonContentsDeployer.Remove()
    {
        RemoveAll();
    }
}