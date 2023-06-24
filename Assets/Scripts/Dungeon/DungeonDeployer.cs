using UnityEngine;
using System.Collections.Generic;
using UniRx;
using System;
using Zenject;
using System.Threading.Tasks;
using Fungus;
using Task = System.Threading.Tasks.Task;

/// <summary>
/// https://note.com/motibe_tsukuru/n/nbe75bb690bcc
/// </summary>

public interface IDungeonDeployer
{
    /// <summary>
    /// マップ
    /// </summary>
    TERRAIN_ID[,] IdMap { get; }

    /// <summary>
    /// マップ（コレクター）
    /// </summary>
    ICollector[,] CellMap { get; }

    /// <summary>
    /// 任意の部屋を入手
    /// </summary>
    /// <param name="roomId"></param>
    /// <returns></returns>
    Room GetRoom(int roomId);

    /// <summary>
    /// ランダムな部屋を取得
    /// </summary>
    /// <returns></returns>
    Room GetRandomRoom();

    /// <summary>
    /// ダンジョンデプロイ
    /// </summary>
    Task DeployDungeon(DungeonElementSetup setup);

    /// <summary>
    /// ダンジョンデプロイ
    /// </summary>
    /// <param name="map"></param>
    /// <param name="range"></param>
    Task DeployDungeon(TERRAIN_ID[,] map, Range range, DungeonElementSetup setup);

    /// <summary>
    /// ダンジョンリムーブ
    /// </summary>
    void RemoveDungeon();

    /// <summary>
    /// ダンジョン生成時
    /// </summary>
    IObservable<TERRAIN_ID[,]> OnDungeonDeploy { get; }

    /// <summary>
    /// ダンジョン除去時
    /// </summary>
    IObservable<Unit> OnDungeonRemove { get; }
}

/// <summary>
/// 部屋クラス
/// </summary>
public class Room
{
    public Room(List<ICollector> cells, Range range)
    {
        Cells = cells;
        Range = range;
    }

    /// <summary>
    /// 部屋のセル
    /// </summary>
    public List<ICollector> Cells { get; }

    /// <summary>
    /// 部屋の範囲
    /// </summary>
    public Range Range { get; }

    /// <summary>
    /// ランダムなセルを返す
    /// </summary>
    /// <returns></returns>
    public ICollector GetRandomCell()
    {
        var index = UnityEngine.Random.Range(0, Cells.Count);
        return Cells[index];
    }

    /// <summary>
    /// 代わりに入れる
    /// </summary>
    /// <param name="remove"></param>
    /// <param name="cell"></param>
    /// <returns></returns>
    public bool TrySetCellInstead(ICollector remove, ICollector cell)
    {
        for (int i = 0; i < Cells.Count; i++)
        {
            if (Cells[i] == remove)
            {
                Cells[i] = cell;
                return true;
            }
        }
        return false;
    }
}

public enum TERRAIN_ID
{
    INVALID = -1,
    WALL = 0,
    PATH_WAY = 1,
    ROOM = 2,
    GATE = 3,
    STAIRS = 4,
}

/*
0 -> 壁
1 -> 通路
2 -> 部屋
3 -> 出入り口
4 -> 階段
-1 -> ダンジョン外

↑
↑
↑
z
 x → → →
*/
public class DungeonDeployer : IDungeonDeployer
{
    [Inject]
    private IDungeonHandler m_DungeonHandler;
    [Inject]
    private IObjectPoolController m_ObjectPoolController;
    [Inject]
    private DungeonProgressHolder m_DungeonProgressHolder;
    [Inject]
    private IDungeonProgressManager m_DungeonProgressManager;
    [Inject]
    private IInstantiater m_Instantiater;

    [Inject]
    private DiContainer m_DiContainer;
    private static IInjector ms_CellInjector = new CellInjector();

    /// <summary>
    /// マップ作成時
    /// </summary>
    private Subject<TERRAIN_ID[,]> m_OnDungeonDeoloy = new Subject<TERRAIN_ID[,]>();
    IObservable<TERRAIN_ID[,]> IDungeonDeployer.OnDungeonDeploy => m_OnDungeonDeoloy;

    /// <summary>
    /// マップ除去時
    /// </summary>
    private Subject<Unit> m_OnDungeonRemove = new Subject<Unit>();
    IObservable<Unit> IDungeonDeployer.OnDungeonRemove => m_OnDungeonRemove;

    /// <summary>
    /// マップ
    /// </summary>
    private TERRAIN_ID[,] m_IdMap;
    TERRAIN_ID[,] IDungeonDeployer.IdMap => m_IdMap;

    /// <summary>
    /// インスタンス
    /// </summary>
    private ICollector[,] m_CellMap;
    ICollector[,] IDungeonDeployer.CellMap => m_CellMap;

    /// <summary>
    /// インスタンス（ルーム限定）
    /// </summary>
    private List<Room> m_RoomList = new List<Room>();

    /// <summary>
    /// 部屋取得
    /// </summary>
    /// <param name="roomId"></param>
    /// <returns></returns>
    Room IDungeonDeployer.GetRoom(int roomId) => m_RoomList[roomId];
    Room IDungeonDeployer.GetRandomRoom() => m_RoomList[UnityEngine.Random.Range(0, m_RoomList.Count)];

    /// <summary>
    /// <see cref="m_RoomList"/> 初期化
    /// </summary>
    private void CreateRoomCellList(List<Range> rangeList)
    {
        m_RoomList = new List<Room>();
        int roomId = 0;
        foreach (Range range in rangeList)
        {
            var list = new List<ICollector>();

            for (int x = range.Start.X; x <= range.End.X; x++)
                for (int z = range.Start.Y; z <= range.End.Y; z++)
                {
                    var cell = m_CellMap[x, z];
                    var info = cell.GetInterface<ICellInfoHandler>();
                    info.RoomId = roomId;
                    list.Add(cell);
                }

            var room = new Room(list, range);
            m_RoomList.Add(room);
            roomId++;
        }
    }

    /// <summary>
    /// ゲートと階段の割り振り
    /// </summary>
    /// <param name="map"></param>
    /// <returns></returns>
    TERRAIN_ID[,] ReworkMap(TERRAIN_ID[,] map)
    {
        // ゲート
        for (int i = 0; i < map.GetLength(0) - 1; i++)
            for (int j = 0; j < map.GetLength(1) - 1; j++)
                if (map[i, j] == TERRAIN_ID.ROOM)
                {
                    var aroundId = map.GetAroundCellId(i, j);
                    if (CheckGateWay(aroundId) == true)
                        map[i, j] = TERRAIN_ID.GATE;
                }

        // 階段
        bool setStairs = false;
        while (setStairs == false)
        {
            var x = UnityEngine.Random.Range(0, map.GetLength(0));
            var z = UnityEngine.Random.Range(0, map.GetLength(1));
            var id = map[x, z];
            if (id == TERRAIN_ID.ROOM)
            {
                map[x, z] = TERRAIN_ID.STAIRS;
                setStairs = true;
            }
        }

        return map;

        /// <summary>
        /// 部屋の入り口になっているかどうかを調べる
        /// </summary>
        /// <param name="aroundGrid"></param>
        /// <returns></returns>
        bool CheckGateWay(AroundCell<TERRAIN_ID> aroundGrid)
        {
            var cells = aroundGrid.AroundCells;

            if (cells[DIRECTION.UP] == TERRAIN_ID.PATH_WAY)
                return true;

            if (cells[DIRECTION.UNDER] == TERRAIN_ID.PATH_WAY)
                return true;

            if (cells[DIRECTION.LEFT] == TERRAIN_ID.PATH_WAY)
                return true;

            if (cells[DIRECTION.RIGHT] == TERRAIN_ID.PATH_WAY)
                return true;

            return false;
        }
    }

    /// <summary>
    /// ダンジョン配備
    /// </summary>
    private async Task DeployDungeon(DungeonElementSetup setup)
    {
        var x = m_DungeonProgressHolder.CurrentDungeonSetup.MapSize.x;
        var y = m_DungeonProgressHolder.CurrentDungeonSetup.MapSize.y;
        var dungeonSetup = m_DungeonProgressHolder.CurrentDungeonSetup;
        var mapInfo = MapGenerator.GenerateMap(x, y, dungeonSetup.RoomCountMin, dungeonSetup.RoomCountMax);
        var map = ReworkMap(mapInfo.Map);

        await DeployDungeonTerrain(map, setup);
        CreateRoomCellList(mapInfo.RangeList);
        await DeployTrap();

        m_OnDungeonDeoloy.OnNext(map);
    }
    async Task IDungeonDeployer.DeployDungeon(DungeonElementSetup setup) => await DeployDungeon(setup);

    /// <summary>
    /// ダンジョン配備（マップ指定）
    /// </summary>
    /// <param name="map"></param>
    /// <param name="range"></param>
    Task IDungeonDeployer.DeployDungeon(TERRAIN_ID[,] map, Range range, DungeonElementSetup setup)
    {
        var rangeList = new List<Range>();
        rangeList.Add(range);
        DeployDungeonTerrain(map, setup);
        CreateRoomCellList(rangeList);

        m_OnDungeonDeoloy.OnNext(map);
        return Task.CompletedTask;
    }

    /// <summary>
    /// ダンジョン撤去
    /// </summary>
    private void RemoveDungeon()
    {
        foreach (var cell in m_CellMap)
            cell.Dispose();

        m_OnDungeonRemove.OnNext(Unit.Default);
        InitializeAllList();
    }
    void IDungeonDeployer.RemoveDungeon() => RemoveDungeon();

    /// <summary>
    /// リスト初期化
    /// </summary>
    private void InitializeAllList()
    {
        m_CellMap = null;
        m_RoomList.Clear();
    }

    /// <summary>
    /// ダンジョンの地形を配置
    /// </summary>
    private Task DeployDungeonTerrain(TERRAIN_ID[,] map, DungeonElementSetup setup)
    {
        int xLength = map.GetLength(0);
        int zLength = map.GetLength(1);
        m_CellMap = new ICollector[xLength, zLength];

        for (int x = 0; x < xLength; x++)
        {
            for (int z = 0; z < zLength; z++)
            {
                var id = map[x, z]; // 古いId
                TERRAIN_ID type = TERRAIN_ID.INVALID; // 新Id
                GameObject cellObject = null; // GameObject
                Vector3 pos = new Vector3Int(x, 0, z);

                switch (id)
                {
                    case TERRAIN_ID.WALL: // 0
                        pos += new Vector3(0, 0.8f, 0);
                        if (m_ObjectPoolController.TryGetObject(TERRAIN_ID.WALL.ToString(), out cellObject) == false)
                            cellObject = m_Instantiater.InstantiatePrefab(setup.Wall, ms_CellInjector);

                        type = TERRAIN_ID.WALL;
                        break;

                    case TERRAIN_ID.PATH_WAY: // 1
                        if (m_ObjectPoolController.TryGetObject(TERRAIN_ID.PATH_WAY.ToString(), out cellObject) == false)
                            cellObject = m_Instantiater.InstantiatePrefab(setup.Path, ms_CellInjector);

                        type = TERRAIN_ID.PATH_WAY;
                        break;

                    case TERRAIN_ID.ROOM: // 2
                        if (m_ObjectPoolController.TryGetObject(TERRAIN_ID.ROOM.ToString(), out cellObject) == false)
                            cellObject = m_Instantiater.InstantiatePrefab(setup.Room, ms_CellInjector);

                        type = TERRAIN_ID.ROOM;
                        break;

                    case TERRAIN_ID.GATE: // 3
                        if (m_ObjectPoolController.TryGetObject(TERRAIN_ID.GATE.ToString(), out cellObject) == false)
                            cellObject = m_Instantiater.InstantiatePrefab(setup.Room, ms_CellInjector);

                        type = TERRAIN_ID.GATE;
                        break;

                    case TERRAIN_ID.STAIRS:
                        if (m_ObjectPoolController.TryGetObject(TERRAIN_ID.STAIRS.ToString(), out cellObject) == false)
                            cellObject = m_Instantiater.InstantiatePrefab(setup.Stairs, ms_CellInjector);

                        type = TERRAIN_ID.STAIRS;
                        break;
                }

                cellObject.transform.position = pos;

                var cell = cellObject.GetComponent<ICollector>();
                var info = cell.GetInterface<ICellInfoHandler>();
                info.CellObject = cellObject;
                info.CellId = type;
                cell.Initialize();
                m_CellMap[x, z] = cell;
            }
        }

        m_IdMap = map;

        return Task.CompletedTask;
    }

    /// <summary>
    /// トラップ
    /// </summary>
    private Task DeployTrap()
    {
        var count = UnityEngine.Random.Range(m_DungeonProgressHolder.CurrentDungeonSetup.TrapCountMin, m_DungeonProgressHolder.CurrentDungeonSetup.TrapCountMax + 1);

        for (int i = 0; i < count; i++)
        {
            // 初期化
            var cell = m_DungeonHandler.GetRandomRoomEmptyCell(); //何もない部屋座標を取得
            var trap = m_DungeonProgressManager.GetRandomTrapSetup();
            var trapHolder = cell.GetInterface<ITrapHandler>();
            trapHolder.SetTrap(trap);
        }
        return Task.CompletedTask;
    }
}