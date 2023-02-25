using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class DungeonContents : Singleton<DungeonContents>
{
    [SerializeField]
    private int m_EnemyCount;

    [SerializeField]
    private int m_ItemCount;

    protected override void Awake()
    {
        base.Awake();
        GameManager.Interface.GetInitEvent.Subscribe(_ => DeployDungeonContents());
    }

    //ダンジョンコンテンツ配置
    public void DeployDungeonContents()
    {
        DeployPlayer();
        DeployEnemy(m_EnemyCount);
        DeployItem(m_ItemCount);
    }

    //ダンジョンコンテンツ撤去
    public void RemoveDungeonContents()
    {
        //RemoveAllPlayerObject();
        RemoveAllEnemyObject();
        RemoveAllItem();
    }

    public void RedeployDungeonContents()
    {
        RedeployPlayer();
        DeployEnemy(5);
    }

    //キャラオブジェクト取得用
    private GameObject CharaObject(CHARA_NAME name)
    {
        if (ObjectPool.Instance.TryGetPoolObject(name.ToString(), out var chara) == false)
            chara = Instantiate(CharaHolder.Instance.CharaObject(name));
        return chara;
    }

    /// <summary>
    ///
    /// プレイヤー関連
    /// 
    /// </summary>

    //プレイヤー配置
    private void DeployPlayer()
    {
        var cell = DungeonHandler.Interface.GetRandomRoomEmptyCell(); //何もない部屋座標を取得

        GameObject player = CharaObject(GameManager.Interface.LeaderName);
        player.transform.position = new Vector3(cell.X, 0.51f, cell.Z);
        var collector = player.GetComponent<ICollector>();
        UnitManager.Interface.AddPlayer(collector);
    }

    //プレイヤー再配置
    private void RedeployPlayer()
    {
        var cell = DungeonHandler.Interface.GetRandomRoomEmptyCell(); //何もない部屋座標を取得

        foreach (ICollector collector in UnitManager.Interface.PlayerList)
        {
            if (collector.RequireComponent<ICharaMove>(out var move) == false)
                continue;

            move.Warp(cell.Position);
        }
    }

    //全てのプレイヤーオブジェクトを撤去
    private void RemoveAllPlayerObject()
    {
        foreach (ICollector player in UnitManager.Interface.PlayerList)
        {
            string name = player.GetComponent<ICharaStatus>().Parameter.GivenName.ToString();
            ObjectPool.Instance.SetObject(name, player.GetComponent<ICharaObjectHolder>().MoveObject);
        }

        UnitManager.Interface.PlayerList.Clear();
    }

    /// <summary>
    ///
    /// 敵関連
    /// 
    /// </summary>

    //敵配置
    private void DeployEnemy(int count)
    {
        if (count <= 0)
            return;

        var map = DungeonManager.Interface.IdMap;

        for (int num = 1; num <= count; num++)
        {
            var cell = DungeonHandler.Interface.GetRandomRoomEmptyCell();
            GameObject enemy = CharaObject(Utility.RandomEnemyName());
            enemy.transform.position = new Vector3(cell.X, 0.51f, cell.Z);
            var collector = enemy.GetComponent<ICollector>();
            UnitManager.Interface.AddEnemy(collector);
        }
    }

    //全ての敵オブジェクトを撤去
    private void RemoveAllEnemyObject()
    {
        foreach (ICollector enemy in UnitManager.Interface.EnemyList)
        {
            string name = enemy.GetComponent<ICharaStatus>().Parameter.GivenName.ToString();
            ObjectPool.Instance.SetObject(name, enemy.GetComponent<ICharaObjectHolder>().MoveObject);
        }

        UnitManager.Interface.EnemyList.Clear();
    }

    //特定の敵オブジェクトを撤去
    private void RemoveEnemyObject(ICollector enemy)
    {
        UnitManager.Interface.EnemyList.Remove(enemy);
        string name = enemy.GetComponent<ICharaStatus>().Parameter.GivenName.ToString();
        ObjectPool.Instance.SetObject(name, enemy.GetComponent<ICharaObjectHolder>().MoveObject);
    }

    /// <summary>
    /// アイテム関連
    /// </summary>
    private GameObject ItemObject(ITEM_NAME name)
    {
        if (ObjectPool.Instance.TryGetPoolObject(name.ToString(), out var item) == false)
            item = Instantiate(ItemHolder.Instance.ItemObject(name));

        return item;
    }

    private void DeployItem(int itemNum)
    {
        if (itemNum <= 0)
            return;

        var map = DungeonManager.Interface.IdMap;

        for (int num = 0; num <= itemNum - 1; num++)
        {
            var cell = DungeonHandler.Interface.GetRandomRoomEmptyCell();
            GameObject obj = ItemObject(Utility.RandomItemName());
            obj.transform.position = new Vector3(cell.X, 0.75f, cell.Z);
            obj.transform.eulerAngles = new Vector3(45f, 0f, 0f);
            IItem item = obj.GetComponent<Item>();
            item.Position = cell.Position;
            ItemManager.Interface.AddItem(item);
        }
    }

    //全てのアイテムオブジェクトを撤去
    private void RemoveAllItem()
    {
        foreach (IItem item in ItemManager.Interface.ItemList)
        {
            string name = item.Name.ToString();
            ObjectPool.Instance.SetObject(name, item.GameObject);
        }
        ItemManager.Interface.ItemList.Clear();
    }

    //特定の敵オブジェクトを撤去
    public void RemoveItem(IItem item)
    {
        item.GameObject.SetActive(false);
        ItemManager.Interface.ItemList.Remove(item);
        string name = item.Name.ToString();
        ObjectPool.Instance.SetObject(name, item.GameObject);
    }
}