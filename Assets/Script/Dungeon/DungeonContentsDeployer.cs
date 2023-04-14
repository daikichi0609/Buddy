using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public enum CONTENTS_TYPE
{
    PLAYER,
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
}

public class DungeonContentsDeployer : Singleton<DungeonContentsDeployer, IDungeonContentsDeployer>, IDungeonContentsDeployer
{
    protected override void Awake()
    {
        base.Awake();
        GameManager.Interface.GetInitEvent.Subscribe(_ => DeployAll()).AddTo(this);
    }

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
                    content = CharaObject(GameManager.Interface.Leader);
                    content.transform.position = new Vector3(info.X, CharaMove.OFFSET_Y, info.Z);
                    var player = content.GetComponent<ICollector>();
                    player.Initialize();
                    if (player.RequireInterface<ICharaStatus>(out var p) == true)
                        p.SetStatus(GameManager.Interface.Leader.Status);
                    UnitHolder.Interface.AddPlayer(player);
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
        var player = UnitHolder.Interface.FriendList[0];
        var content = player.GetInterface<ICharaObjectHolder>().MoveObject;
        var info = cell.GetInterface<ICellInfoHolder>();
        content.transform.position = new Vector3(info.X, CharaMove.OFFSET_Y, info.Z);
        player.Initialize();
        UnitHolder.Interface.AddPlayer(player);
    }

    private void DeployAll()
    {
        Debug.Log("Deploy Contents");
        Deploy(CONTENTS_TYPE.PLAYER);
        Deploy(CONTENTS_TYPE.ENEMY, DungeonProgressManager.Interface.CurrentDungeonSetup.EnemyCountMax);
        Deploy(CONTENTS_TYPE.ITEM, DungeonProgressManager.Interface.CurrentDungeonSetup.ItemCountMax);
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