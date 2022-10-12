using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class DungeonContents : SingletonMonoBehaviour<DungeonContents>
{
    protected override void Awake()
    {
        base.Awake();
        GameManager.Instance.GetInit.Subscribe(_ => DeployDungeonContents());
    }

    //ダンジョンコンテンツ配置
    public void DeployDungeonContents()
    {
        DeployPlayer();
        DeployEnemy(5);
        DeployItem(5);
    }

    //ダンジョンコンテンツ撤去
    public void RemoveDungeonContents()
    {
        //RemoveAllPlayerObject();
        RemoveAllEnemyObject();
        RemoveAllItemObject();
    }

    public void RedeployDungeonContents()
    {
        RedeployPlayer();
        DeployEnemy(5);
    }

    //キャラオブジェクト取得用
    private GameObject CharaObject(Define.CHARA_NAME name)
    {
        GameObject chara = ObjectPool.Instance.PoolObject(name.ToString());
        if (chara == null)
        {
            chara = Instantiate(CharaHolder.Instance.CharaObject(name));
        }
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
        int[,] map = DungeonTerrain.Instance.Map; //マップ取得
        int[] coord = ChooseEmptyRandomRoomGrid(map); //何もない部屋座標を取得

        GameObject player = CharaObject(GameManager.Instance.LeaderName);
        player.transform.position = new Vector3(coord[0], 0.51f, coord[1]);
        ObjectManager.Instance.m_PlayerList.Add(player);
        player.GetComponent<Chara>().Initialize(20);
        player.GetComponent<CharaBattle>().Initialize();
        CharaUiManager.Instance.GenerateCharacterUi(player);
    }

    //プレイヤー再配置
    private void RedeployPlayer()
    {
        int[,] map = DungeonTerrain.Instance.Map; //マップ取得
        int[] coord = ChooseEmptyRandomRoomGrid(map); //何もない部屋座標を取得

        GameObject player = ObjectManager.Instance.m_PlayerList[0];
        player.transform.position = new Vector3(coord[0], 0.51f, coord[1]);
        player.GetComponent<Chara>().Position = player.transform.position;
        player.GetComponent<Chara>().Direction = new Vector3(0, 0, -1);
    }

    //全てのプレイヤーオブジェクトを撤去
    private void RemoveAllPlayerObject()
    {
        foreach(GameObject player in ObjectManager.Instance.m_PlayerList)
        {
            ObjectManager.Instance.m_PlayerList.Remove(player);
            string name = player.GetComponent<CharaBattle>().Parameter.Name.ToString();
            ObjectPool.Instance.SetObject(name, player);
        }

        ObjectManager.Instance.m_PlayerList.Clear();
    }

    /// <summary>
    ///
    /// 敵関連
    /// 
    /// </summary>

    //敵配置
    private void DeployEnemy(int enemyNum)
    {
        if (enemyNum <= 0)
            return;

        int[,] map = DungeonTerrain.Instance.Map;

        for (int num = 1; num <= enemyNum; num++)
        {
            int[] coord = ChooseEmptyRandomRoomGrid(map);
            GameObject enemy = CharaObject(Utility.RandomEnemyName());
            enemy.transform.position = new Vector3(coord[0], 0.51f, coord[1]);
            ObjectManager.Instance.m_EnemyList.Add(enemy);
            enemy.GetComponent<Chara>().Initialize(1);
            enemy.GetComponent<CharaBattle>().Initialize();
        }
    }

    //全ての敵オブジェクトを撤去
    private void RemoveAllEnemyObject()
    {
        foreach (GameObject enemy in ObjectManager.Instance.m_EnemyList)
        {
            string name = enemy.GetComponent<CharaBattle>().Parameter.Name.ToString();
            ObjectPool.Instance.SetObject(name, enemy);
        }
        ObjectManager.Instance.m_EnemyList.Clear();
    }

    //特定の敵オブジェクトを撤去
    public void RemoveEnemyObject(GameObject enemy)
    {
        enemy.SetActive(false);
        ObjectManager.Instance.m_EnemyList.Remove(enemy);
        string name = enemy.GetComponent<CharaBattle>().Parameter.Name.ToString();
        ObjectPool.Instance.SetObject(name, enemy);
    }

    /// <summary>
    ///
    /// アイテム関連
    /// 
    /// </summary>
    
    private GameObject ItemObject(Define.ITEM_NAME name)
    {
        GameObject item = ObjectPool.Instance.PoolObject(name.ToString());
        if(item == null)
        {
            item = Instantiate(ItemHolder.Instance.ItemObject(name));
        }
        return item;
    }

    private void DeployItem(int itemNum)
    {
        if (itemNum <= 0)
            return;

        int[,] map = DungeonTerrain.Instance.Map;

        for (int num = 0; num <= itemNum - 1; num++)
        {
            int[] coord = ChooseEmptyRandomRoomGrid(map);
            GameObject item = ItemObject(Utility.RandomItemName());
            item.transform.position = new Vector3(coord[0], 0.75f, coord[1]);
            item.transform.eulerAngles = new Vector3(45f, 0f, 0f);
            item.GetComponent<Item>().Position = new Vector3(coord[0], 0f, coord[1]);
            ObjectManager.Instance.ItemList.Add(item);
        }
    }

    //全てのアイテムオブジェクトを撤去
    private void RemoveAllItemObject()
    {
        foreach (GameObject item in ObjectManager.Instance.ItemList)
        {
            string name = item.GetComponent<Item>().Name.ToString();
            ObjectPool.Instance.SetObject(name, item);
        }
        ObjectManager.Instance.ItemList = new List<GameObject>();
    }

    //特定の敵オブジェクトを撤去
    public void RemoveItemObject(GameObject item)
    {
        item.SetActive(false);
        ObjectManager.Instance.ItemList.Remove(item);
        string name = item.GetComponent<Item>().Name.ToString();
        ObjectPool.Instance.SetObject(name, item);
    }

    //ランダムな部屋座標を返す
    private int[] ChooseRandamRoomGrid(int[,] map)
    {
        int num = -1;
        int x = -1;
        int z = -1;
        while (num != 2)
        {
            x = Random.Range(0, map.GetLength(0));
            z = Random.Range(0, map.GetLength(1));
            num = map[x, z];
        }
        int[] coord = { x, z };
        return coord;
    }

    //ランダムな何も乗っていない部屋座標を返す
    public int[] ChooseEmptyRandomRoomGrid(int[,] map)
    {
        int[] coord = ChooseRandamRoomGrid(map);
        bool isEmpty = false;
        while(isEmpty == false)
        {
            coord = ChooseRandamRoomGrid(map);
            isEmpty = Positional.IsNoOneThere(new Vector3(coord[0], 0, coord[1]));
        }
        return coord;
    }
}