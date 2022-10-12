using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using System.Linq;

public class ObjectManager : SingletonMonoBehaviour<ObjectManager>
{
    /// <summary>
    /// 要素の追加
    /// 要素の削除
    /// 要素数の変化
    /// 要素の上書き
    /// 要素の移動
    /// リストのクリア
    /// </summary>

    protected override void Awake()
    {
        base.Awake();

        GetPlayerObs.Subscribe(_ =>
        {
            Debug.Log("プレイヤーリスト操作");
        });

        GetEnemyObs.Subscribe(_ =>
        {
            Debug.Log("敵リスト操作");
        });
    }

    /// <summary>
    /// Playerのオブジェクト管理リスト
    /// </summary>
    public ReactiveCollection<GameObject> m_PlayerList = new ReactiveCollection<GameObject>();

    /// <summary>
    /// IObservable取得用
    /// </summary>
    public IObservable<CollectionReplaceEvent<GameObject>> GetPlayerObs => m_PlayerList.ObserveReplace();

    /// <summary>
    /// リストの値だけ見れる
    /// </summary>
    [SerializeField]
    public List<GameObject> PlayerValueList => m_PlayerList.ToList();

    /// <summary>
    /// Playerオブジェクト取得
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public GameObject PlayerObject(int i)
    {
        return m_PlayerList[i];
    }

    /// <summary>
    /// 特定の座標のプレイヤーを取得
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public GameObject SpecifiedPositionPlayerObject(Vector3 pos)
    {
        foreach (GameObject player in m_PlayerList)
        {
            Chara charaMove = player.GetComponent<Chara>();
            if (charaMove.Position.x == pos.x && charaMove.Position.z == pos.z)
            {
                return player;
            }
        }
        return null;
    }

    /// <summary>
    /// 特定の部屋にいるプレイヤーを取得
    /// </summary>
    /// <param name="roomId"></param>
    /// <returns></returns>
    public List<GameObject> SpecifiedRoomPlayerObjectList(int roomId)
    {
        if(roomId <= 0)
        {
            return null;
        }

        List<GameObject> playerList = new List<GameObject>();
        List<GameObject> roomList = DungeonTerrain.Instance.GetRoomList(roomId);
        foreach (GameObject player in m_PlayerList)
        {
            Chara chara = player.GetComponent<Chara>();
            foreach(GameObject grid in roomList)
            {
                if(chara.Position.x == grid.transform.position.x && chara.Position.z == grid.transform.position.z)
                {
                    playerList.Add(player);
                }
            }
        }

        return playerList;
    }

    /// <summary>
    /// Enemyのオブジェクト管理リスト
    /// </summary>
    public ReactiveCollection<GameObject> m_EnemyList = new ReactiveCollection<GameObject>();

    /// <summary>
    /// IObservable取得用
    /// </summary>
    public IObservable<CollectionReplaceEvent<GameObject>> GetEnemyObs => m_EnemyList.ObserveReplace();

    /// <summary>
    /// リストの値だけ見れる
    /// </summary>
    [SerializeField]
    public List<GameObject> EnemyValueList => m_EnemyList.ToList();

    /// <summary>
    /// Enemyオブジェクト取得
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public GameObject EnemyObject(int i)
    {
        return m_EnemyList[i];
    }

    /// <summary>
    /// 特定の座標の敵を取得
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public GameObject SpecifiedPositionEnemyObject(Vector3 pos)
    {
        foreach (GameObject enemy in m_EnemyList)
        {
            Chara charaMove = enemy.GetComponent<Chara>();
            if (charaMove.Position.x == pos.x && charaMove.Position.z == pos.z)
            {
                return enemy;
            }
        }
        return null;
    }

    /// <summary>
    /// 特定の部屋の敵を取得
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public GameObject SpecifiedPositionCharacterObject(Vector3 pos)
    {
        foreach (GameObject player in m_PlayerList)
        {
            Chara charaMove = player.GetComponent<Chara>();
            if (charaMove.Position.x == pos.x && charaMove.Position.z == pos.z)
            {
                return player;
            }
        }

        foreach (GameObject enemy in m_EnemyList)
        {
            Chara charaMove = enemy.GetComponent<Chara>();
            if (charaMove.Position.x == pos.x && charaMove.Position.z == pos.z)
            {
                return enemy;
            }
        }
        return null;
    }

    /// <summary>
    /// 落ちているアイテムリスト
    /// </summary>
    [SerializeField] private List<GameObject> m_ItemList = new List<GameObject>();
    public List<GameObject> ItemList
    {
        get { return m_ItemList; }
        set { m_ItemList = value; }
    }

    /// <summary>
    /// 特定の座標のアイテムを取得
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public GameObject SpecifiedPositionItemObject(Vector3 pos)
    {
        foreach (GameObject player in m_PlayerList)
        {
            Chara charaMove = player.GetComponent<Chara>();
            if (charaMove.Position.x == pos.x && charaMove.Position.z == pos.z)
            {
                return player;
            }
        }

        foreach (GameObject enemy in m_EnemyList)
        {
            Chara charaMove = enemy.GetComponent<Chara>();
            if (charaMove.Position.x == pos.x && charaMove.Position.z == pos.z)
            {
                return enemy;
            }
        }
        return null;
    }

    /// <summary>
    /// 特定の部屋の出入り口を取得
    /// </summary>
    /// <param name="roomId"></param>
    /// <returns></returns>
    public List<GameObject> GateWayObjectList(int roomId)
    {
        List<GameObject> roomList = DungeonTerrain.Instance.GetRoomList(roomId);
        List<GameObject> list = new List<GameObject>();
        foreach (GameObject gridObject in roomList)
        {
            Grid grid = gridObject.GetComponent<Grid>();
            if (grid.GridID == DungeonTerrain.GRID_ID.GATE)
            {
                list.Add(gridObject);
            }
        }
        return list;
    }
}
