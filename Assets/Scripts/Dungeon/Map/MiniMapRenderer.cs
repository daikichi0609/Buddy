using System;
using System.Collections;
using System.Collections.Generic;
using Fungus;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using static UnityEditor.PlayerSettings;

public interface IMiniMapRenderer
{
    /// <summary>
    /// アイコン登録
    /// </summary>
    /// <returns></returns>
    IDisposable RegisterIcon(ICollector collector);

    /// <summary>
    /// アイコン更新
    /// </summary>
    /// <param name="collector"></param>
    void ReflectIcon(ICollector collector);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="move"></param>
    void SetPlayerPos(Vector3Int pos);
}

public class MiniMapRenderer : MonoBehaviour, IMiniMapRenderer
{
    [Inject]
    private IObjectPoolController m_ObjectPoolController;
    [Inject]
    private IDungeonHandler m_DungeonHandler;
    [Inject]
    private IDungeonDeployer m_DungeonDeployer;

    [SerializeField]
    private GameObject m_MiniMapwindow;
    [SerializeField]
    private GameObject m_Parent;

    [SerializeField]
    private GameObject m_RoadImage;
    [SerializeField]
    private GameObject m_FriendImage;
    [SerializeField]
    private GameObject m_EnemyImage;
    [SerializeField]
    private GameObject m_ItemImage;

    private float Pw { get; set; }
    private float Ph { get; set; }
    private int MapSizeX { get; set; }
    private int MapSizeY { get; set; }

    private static readonly string KEY_ROAD = "RoadImage";
    private static readonly string KEY_FRIEND = "FriendImage";
    private static readonly string KEY_ENEMY = "EnemyImage";
    private static readonly string KEY_ITEM = "ItemImage";

    private static readonly int VISIBLE_RANGE = 2;

    /// <summary>
    /// プレイヤーの座標
    /// </summary>
    private ReactiveProperty<Vector3Int> m_PlayerPos = new ReactiveProperty<Vector3Int>();

    /// <summary>
    /// ミニマップオブジェクト
    /// </summary>
    private Dictionary<(int, int), GameObject> m_MapTerrains = new Dictionary<(int, int), GameObject>();

    /// <summary>
    /// フレンド
    /// </summary>
    private Dictionary<ICollector, GameObject> m_FriendIcons = new Dictionary<ICollector, GameObject>();

    /// <summary>
    /// 敵
    /// </summary>
    private Dictionary<ICollector, GameObject> m_EnemyIcons = new Dictionary<ICollector, GameObject>();

    /// <summary>
    /// アイテム
    /// </summary>
    private Dictionary<ICollector, GameObject> m_ItemIcons = new Dictionary<ICollector, GameObject>();

    [Inject]
    private void Construct(IDungeonDeployer dungeonDeployer)
    {
        RectTransform rect = m_RoadImage.GetComponent<RectTransform>();
        Pw = rect.sizeDelta.x;
        Ph = rect.sizeDelta.y;

        dungeonDeployer.OnDungeonDeploy.SubscribeWithState(this, (map, self) => self.Mapping(map)).AddTo(this);
        dungeonDeployer.OnDungeonRemove.SubscribeWithState(this, (_, self) => self.ResetMiniMap()).AddTo(this);
    }

    private void Start()
    {
        m_PlayerPos.SubscribeWithState(this, (pos, self) =>
        {
            self.m_DungeonHandler.TryGetRoomId(pos, out var playerRoom);

            foreach (var e in self.m_EnemyIcons)
            {
                if (e.Key.RequireInterface<ICharaMove>(out var move) == false)
                    continue;

                var targetPos = move.Position;

                if (self.m_DungeonHandler.TryGetRoomId(targetPos, out var id) == true && id == playerRoom)
                {
                    e.Value.SetActive(true);
                    continue;
                }

                bool isActive = self.IsVisibleFromPlayer(targetPos.x, targetPos.z);
                e.Value.SetActive(isActive);
            }

            foreach (var i in self.m_ItemIcons)
            {
                if (i.Key.RequireInterface<IItemHandler>(out var item) == false)
                    continue;

                var targetPos = item.Position;

                if (self.m_DungeonHandler.TryGetRoomId(targetPos, out var id) == true && id == playerRoom)
                {
                    i.Value.SetActive(true);
                    continue;
                }

                bool isActive = self.IsVisibleFromPlayer(targetPos.x, targetPos.z);
                i.Value.SetActive(isActive);
            }
        }).AddTo(this);
    }

    /// <summary>
    /// アイコン登録
    /// </summary>
    /// <param name="collector"></param>
    IDisposable IMiniMapRenderer.RegisterIcon(ICollector collector)
    {
        GameObject icon = null;

        if (collector.RequireInterface<ICharaMove>(out var move) == true && collector.RequireInterface<ICharaTypeHolder>(out var type) == true)
        {
            if (type.Type == CHARA_TYPE.FRIEND)
            {
                if (m_ObjectPoolController.TryGetObject(KEY_FRIEND, out icon) == false)
                    icon = Instantiate(m_FriendImage, m_Parent.transform);

                m_FriendIcons.Add(collector, icon);
                ReflectIcon(collector);
                return Disposable.CreateWithState((this, collector), tuple =>
                {
                    if (tuple.Item1.m_FriendIcons.Remove(tuple.collector, out var icon) == true)
                        tuple.Item1.m_ObjectPoolController.SetObject(KEY_FRIEND, icon);
                });
            }
            else if (type.Type == CHARA_TYPE.ENEMY)
            {
                if (m_ObjectPoolController.TryGetObject(KEY_ENEMY, out icon) == false)
                    icon = Instantiate(m_EnemyImage, m_Parent.transform);

                m_EnemyIcons.Add(collector, icon);
                ReflectIcon(collector);
                return Disposable.CreateWithState((this, collector), tuple =>
                {
                    if (tuple.Item1.m_EnemyIcons.Remove(tuple.collector, out var icon) == true)
                        tuple.Item1.m_ObjectPoolController.SetObject(KEY_ENEMY, icon);
                });
            }
        }

        if (collector.RequireInterface<IItemHandler>(out var item) == true)
        {
            if (m_ObjectPoolController.TryGetObject(KEY_ITEM, out icon) == false)
                icon = Instantiate(m_ItemImage, m_Parent.transform);

            m_ItemIcons.Add(collector, icon);
            ReflectIcon(collector);
            return Disposable.CreateWithState((this, collector), tuple =>
            {
                if (tuple.Item1.m_ItemIcons.Remove(tuple.collector, out var icon) == true)
                    tuple.Item1.m_ObjectPoolController.SetObject(KEY_ITEM, icon);
            });
        }

        return Disposable.Empty;
    }

    /// <summary>
    /// アイコン更新
    /// </summary>
    /// <param name="collector"></param>
    private void ReflectIcon(ICollector collector)
    {
        GameObject icon = null;

        if (collector.RequireInterface<ICharaMove>(out var move) == true)
        {
            if (m_FriendIcons.TryGetValue(collector, out icon) == true)
                RenderIcon(icon, move.Position.x, move.Position.z);
            else if (m_EnemyIcons.TryGetValue(collector, out icon) == true)
            {
                icon.SetActive(IsVisibleFromPlayer(move.Position.x, move.Position.z));
                RenderIcon(icon, move.Position.x, move.Position.z);
            }
        }
        else if (collector.RequireInterface<IItemHandler>(out var item) == true)
            if (m_ItemIcons.TryGetValue(collector, out icon) == true)
            {
                icon.SetActive(IsVisibleFromPlayer(item.Position.x, item.Position.z));
                RenderIcon(icon, item.Position.x, item.Position.z);
            }
    }
    void IMiniMapRenderer.ReflectIcon(ICollector collector) => ReflectIcon(collector);

    /// <summary>
    /// プレイヤー座標の更新
    /// </summary>
    void IMiniMapRenderer.SetPlayerPos(Vector3Int pos)
    {
        if (m_DungeonHandler.TryGetRoomId(pos, out var id) == true)
        {
            var room = m_DungeonDeployer.GetRoom(id);
            foreach (var cell in room.Cells)
            {
                var info = cell.GetInterface<ICellInfoHandler>();
                var cellPos = info.Position;
                if (m_MapTerrains.TryGetValue((cellPos.x, cellPos.z), out var icon) == true)
                    icon.SetActive(true);

                if (info.CellId == TERRAIN_ID.GATE)
                {
                    for (int x = cellPos.x - 1; x <= cellPos.x + 1; x++)
                        for (int z = cellPos.z - 1; z <= cellPos.z + 1; z++)
                        {
                            if (m_MapTerrains.TryGetValue((x, z), out var around) == true)
                                around.SetActive(true);
                        }
                }
            }
        }

        for (int x = pos.x - VISIBLE_RANGE; x <= pos.x + VISIBLE_RANGE; x++)
            for (int z = pos.z - VISIBLE_RANGE; z <= pos.z + VISIBLE_RANGE; z++)
            {
                if (m_MapTerrains.TryGetValue((x, z), out var icon) == true)
                    icon.SetActive(true);
            }

        m_PlayerPos.Value = pos;
    }

    /// <summary>
    /// 範囲内にいるか
    /// </summary>
    /// <param name="x"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    private bool IsVisibleFromPlayer(int x, int z)
    {
        int playerX = m_PlayerPos.Value.x;
        int playerZ = m_PlayerPos.Value.z;

        return x >= playerX - VISIBLE_RANGE && x <= playerX + VISIBLE_RANGE && z >= playerZ - VISIBLE_RANGE && z <= playerZ + VISIBLE_RANGE;
    }

    /// <summary>
    /// 地形をマッピングする
    /// </summary>
    /// <param name="map"></param>
    private void Mapping(TERRAIN_ID[,] map)
    {
        MapSizeX = map.GetLength(0);
        MapSizeY = map.GetLength(1);

        m_MiniMapwindow.GetComponent<RectTransform>().sizeDelta = new Vector2(MapSizeX * Pw, MapSizeY * Ph);

        for (int x = 0; x < map.GetLength(0); x++)
            for (int y = 0; y < map.GetLength(1); y++)
            {
                var id = map[x, y];
                if (id == TERRAIN_ID.WALL)
                    continue;

                if (m_ObjectPoolController.TryGetObject(KEY_ROAD, out var passable) == false)
                    passable = Instantiate(m_RoadImage, m_Parent.transform);

                passable.SetActive(false);
                RenderIcon(passable, x, y);
                m_MapTerrains.Add((x, y), passable);
            }
    }

    /// <summary>
    /// アイコンを表示する
    /// </summary>
    /// <param name="icon"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    private void RenderIcon(GameObject icon, int x, int y)
    {
        var diffX = x - MapSizeX / 2;
        var diffY = y - MapSizeY / 2;
        icon.GetComponent<RectTransform>().anchoredPosition = new Vector2(Pw * diffX, Ph * diffY);
    }

    /// <summary>
    /// マップをリセットする
    /// </summary>
    private void ResetMiniMap()
    {
        foreach (var passable in m_MapTerrains.Values)
            m_ObjectPoolController.SetObject(KEY_ROAD, passable);

        foreach (var friend in m_FriendIcons.Values)
            m_ObjectPoolController.SetObject(KEY_FRIEND, friend);

        foreach (var enemy in m_EnemyIcons.Values)
            m_ObjectPoolController.SetObject(KEY_ENEMY, enemy);

        foreach (var item in m_ItemIcons.Values)
            m_ObjectPoolController.SetObject(KEY_ITEM, item);

        m_MapTerrains.Clear();
        m_FriendIcons.Clear();
        m_EnemyIcons.Clear();
        m_ItemIcons.Clear();
    }
}