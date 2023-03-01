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
    [SerializeField]
    private int m_EnemyCount;

    [SerializeField]
    private int m_ItemCount;

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
    private GameObject CharaObject(CHARA_NAME name)
    {
        if (ObjectPool.Instance.TryGetPoolObject(name.ToString(), out var chara) == false)
            chara = Instantiate(Resources.Load<GameObject>("Prefab/Character/" + name.ToString()));

        return chara;
    }

    /// <summary>
    /// アイテムオブジェクト取得
    /// </summary>
    private GameObject ItemObject(ITEM_NAME name)
    {
        if (ObjectPool.Instance.TryGetPoolObject(name.ToString(), out var item) == false)
            item = Instantiate(ItemHolder.Instance.ItemObject(name));

        return item;
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
            GameObject content = null;

            // ゲームオブジェクト取得
            switch (type)
            {
                case CONTENTS_TYPE.PLAYER:
                    content = CharaObject(GameManager.Interface.LeaderName);
                    content.transform.position = new Vector3(cell.X, CharaMove.OFFSET_Y, cell.Z);
                    var player = content.GetComponent<ICollector>();
                    player.Initialize();
                    UnitManager.Interface.AddPlayer(player);
                    break;

                case CONTENTS_TYPE.ENEMY:
                    content = CharaObject(Utility.RandomEnemyName());
                    content.transform.position = new Vector3(cell.X, CharaMove.OFFSET_Y, cell.Z);
                    var enemy = content.GetComponent<ICollector>();
                    enemy.Initialize();
                    UnitManager.Interface.AddEnemy(enemy);
                    break;

                case CONTENTS_TYPE.ITEM:
                    content = ItemObject(Utility.RandomItemName());
                    content.transform.position = new Vector3(cell.X, Item.OFFSET_Y, cell.Z);
                    content.transform.eulerAngles = new Vector3(45f, 0f, 0f);
                    IItem item = content.GetComponent<Item>();
                    item.Position = cell.Position;
                    ItemManager.Interface.AddItem(item);
                    break;
            }
        }
    }

    private void DeployAll()
    {
        Deploy(CONTENTS_TYPE.PLAYER);
        Deploy(CONTENTS_TYPE.ENEMY, m_EnemyCount);
        Deploy(CONTENTS_TYPE.ITEM, m_ItemCount);
    }
    void IDungeonContentsDeployer.Deploy() => DeployAll();

    //ダンジョンコンテンツ撤去
    private void RemoveAll()
    {
        // ----- Player ----- //
        for (int i = 0; i < UnitManager.Interface.PlayerList.Count; i++)
        {
            var player = UnitManager.Interface.PlayerList[i];
            UnitManager.Interface.RemoveUnit(player);
            string name = player.GetInterface<ICharaStatus>().Parameter.GivenName.ToString();
            ObjectPool.Instance.SetObject(name, player.GetInterface<ICharaObjectHolder>().MoveObject);
            player.Dispose();
        }

        // ----- Enemy ----- //
        for (int i = 0; i < UnitManager.Interface.EnemyList.Count; i++)
        {
            var enemy = UnitManager.Interface.EnemyList[i];
            UnitManager.Interface.RemoveUnit(enemy);
            string name = enemy.GetInterface<ICharaStatus>().Parameter.GivenName.ToString();
            ObjectPool.Instance.SetObject(name, enemy.GetInterface<ICharaObjectHolder>().MoveObject);
            enemy.Dispose();
        }

        // ----- Item ----- //
        for (int i = 0; i < ItemManager.Interface.ItemList.Count; i++)
        {
            var item = ItemManager.Interface.ItemList[i];
            ItemManager.Interface.RemoveItem(item);
            string name = item.Name.ToString();
            ObjectPool.Instance.SetObject(name, item.ItemObject);
        }
    }

    void IDungeonContentsDeployer.Remove()
    {
        RemoveAll();
    }
}