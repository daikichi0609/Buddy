using UnityEngine;
using System.Collections.Generic;
using UniRx;
using System;

/// <summary>
/// https://note.com/motibe_tsukuru/n/nbe75bb690bcc
/// </summary>

public interface IDungeonDeployer : ISingleton
{
    /// <summary>
    /// マップ
    /// </summary>
    TERRAIN_ID[,] IdMap { get; }

    /// <summary>
    /// マップ（コレクター）
    /// </summary>
    List<List<ICollector>> CellMap { get; }

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
    void DeployDungeon(DungeonElementSetup setup);

    /// <summary>
    /// ダンジョンデプロイ
    /// </summary>
    /// <param name="map"></param>
    /// <param name="range"></param>
    void DeployDungeon(TERRAIN_ID[,] map, Range range, DungeonElementSetup setup);

    /// <summary>
    /// ダンジョンリムーブ
    /// </summary>
    void RemoveDungeon();
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
public class DungeonDeployer : Singleton<DungeonDeployer, IDungeonDeployer>, IDungeonDeployer
{
    /// <summary>
    /// マップ
    /// </summary>
    private TERRAIN_ID[,] m_IdMap;
    TERRAIN_ID[,] IDungeonDeployer.IdMap => m_IdMap;

    private void OverWriteCellId(TERRAIN_ID value, int x, int z) => m_IdMap[x, z] = value;

    /// <summary>
    /// インスタンス
    /// </summary>
    private List<List<ICollector>> m_CellMap = new List<List<ICollector>>();
    List<List<ICollector>> IDungeonDeployer.CellMap => m_CellMap;

    private void SetCellInstead(ICollector cell, int x, int z)
    {
        var remove = m_CellMap[x][z];
        m_CellMap[x].RemoveAt(z);
        m_CellMap[x].Insert(z, cell);

        foreach (var room in m_RoomList)
        {
            if (room.TrySetCellInstead(remove, cell) == true)
                break;
        }

        remove.Dispose();
    }

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
                    var cell = m_CellMap[x][z];
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
    /// ダンジョン配備
    /// </summary>
    private void DeployDungeon(DungeonElementSetup setup)
    {
        var x = DungeonProgressManager.Interface.CurrentDungeonSetup.MapSize.x;
        var y = DungeonProgressManager.Interface.CurrentDungeonSetup.MapSize.y;
        var roomCount = DungeonProgressManager.Interface.CurrentDungeonSetup.RoomCountMax;
        var mapInfo = MapGenerator.GenerateMap(x, y, roomCount);
        m_IdMap = mapInfo.Map;
# if DEBUG
        Debug.Log("Map Reload");
#endif

        DeployDungeonTerrain(setup);
        CreateRoomCellList(mapInfo.RangeList);
        DeployStairs(setup);
        DeployTrap();
    }
    void IDungeonDeployer.DeployDungeon(DungeonElementSetup setup) => DeployDungeon(setup);

    /// <summary>
    /// ダンジョン配備（マップ指定）
    /// </summary>
    /// <param name="map"></param>
    /// <param name="range"></param>
    void IDungeonDeployer.DeployDungeon(TERRAIN_ID[,] map, Range range, DungeonElementSetup setup)
    {
        m_IdMap = map;
        var rangeList = new List<Range>();
        rangeList.Add(range);
        DeployDungeonTerrain(setup);
        CreateRoomCellList(rangeList);
    }

    /// <summary>
    /// ダンジョン撤去
    /// </summary>
    private void RemoveDungeon()
    {
        foreach (var list in m_CellMap)
            foreach (var cell in list)
                cell.Dispose();

        InitializeAllList();
    }
    void IDungeonDeployer.RemoveDungeon() => RemoveDungeon();

    /// <summary>
    /// リスト初期化
    /// </summary>
    private void InitializeAllList()
    {
        m_CellMap.Clear();
        m_RoomList.Clear();
    }

    /// <summary>
    /// ダンジョンの地形を配置
    /// </summary>
    private void DeployDungeonTerrain(DungeonElementSetup setup)
    {
        for (int i = 0; i < m_IdMap.GetLength(0) - 1; i++)
        {
            m_CellMap.Add(new List<ICollector>());

            for (int j = 0; j < m_IdMap.GetLength(1) - 1; j++)
            {
                var id = m_IdMap[i, j]; // 古いId
                TERRAIN_ID type = TERRAIN_ID.INVALID; // 新Id
                GameObject cellObject = null; // GameObject
                Vector3 pos = new Vector3Int(i, 0, j);

                switch (id)
                {
                    case TERRAIN_ID.WALL: // 0
                        pos += new Vector3(0, 0.8f, 0);
                        if (ObjectPoolController.Interface.TryGetObject(TERRAIN_ID.WALL.ToString(), out cellObject) == false)
                            cellObject = Instantiate(setup.Wall, pos, Quaternion.identity);
                        else
                            cellObject.transform.position = pos;

                        type = TERRAIN_ID.WALL;
                        break;

                    case TERRAIN_ID.PATH_WAY: // 1
                        if (ObjectPoolController.Interface.TryGetObject(TERRAIN_ID.PATH_WAY.ToString(), out cellObject) == false)
                            cellObject = Instantiate(setup.Path, pos, Quaternion.identity);
                        else
                            cellObject.transform.position = pos;

                        type = TERRAIN_ID.PATH_WAY;
                        break;

                    case TERRAIN_ID.ROOM: // 2
                        AroundCellId aroundId = DungeonHandler.Interface.GetAroundCellId(i, j);
                        if (CheckGateWay(aroundId) == true)
                        {
                            m_IdMap[i, j] = TERRAIN_ID.GATE; // 入口なら設定し直す

                            if (ObjectPoolController.Interface.TryGetObject(TERRAIN_ID.GATE.ToString(), out cellObject) == false)
                                cellObject = Instantiate(setup.Room, pos, Quaternion.identity);
                            else
                                cellObject.transform.position = pos;

                            type = TERRAIN_ID.GATE;
                        }
                        else
                        {
                            if (ObjectPoolController.Interface.TryGetObject(TERRAIN_ID.ROOM.ToString(), out cellObject) == false)
                                cellObject = Instantiate(setup.Room, pos, Quaternion.identity);
                            else
                                cellObject.transform.position = pos;

                            type = TERRAIN_ID.ROOM;
                        }
                        break;
                }

                var cell = cellObject.GetComponent<ICollector>();
                var info = cell.GetInterface<ICellInfoHandler>();
                info.CellObject = cellObject;
                info.CellId = type;
                cell.Initialize();
                m_CellMap[i].Add(cell);
            }
        }

        /// <summary>
        /// 部屋の入り口になっているかどうかを調べる
        /// </summary>
        /// <param name="aroundGrid"></param>
        /// <returns></returns>
        bool CheckGateWay(AroundCellId aroundGrid)
        {
            var cells = aroundGrid.Cells;

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
    /// 4 -> 階段
    /// </summary>
    private void DeployStairs(DungeonElementSetup setup) //階段配置
    {
        var pos = DungeonHandler.Interface.GetRandomRoomEmptyCellPosition(); // 何もない部屋座標を取得
        var x = pos.x;
        var z = pos.z;

        OverWriteCellId(TERRAIN_ID.STAIRS, x, z); // マップに階段を登録

        var currentDungeonSetup = DungeonProgressManager.Interface.CurrentDungeonSetup;

        if (ObjectPoolController.Interface.TryGetObject(TERRAIN_ID.STAIRS.ToString(), out var cellObject) == false)
            cellObject = Instantiate(setup.Stairs, new Vector3(x, 0, z), Quaternion.identity); //オブジェクト生成
        else
            cellObject.transform.position = new Vector3(x, 0, z);

        var cell = cellObject.GetComponent<ICollector>();
        var info = cell.GetInterface<ICellInfoHandler>();
        info.CellObject = cellObject;
        info.CellId = TERRAIN_ID.STAIRS;
        SetCellInstead(cell, x, z); // 既存のオブジェクトの代わりに代入
    }

    /// <summary>
    /// トラップ
    /// </summary>
    private void DeployTrap()
    {
        var count = UnityEngine.Random.Range(DungeonProgressManager.Interface.CurrentDungeonSetup.TrapCountMin, DungeonProgressManager.Interface.CurrentDungeonSetup.TrapCountMax + 1);

        for (int i = 0; i < count; i++)
        {
            // 初期化
            var cell = DungeonHandler.Interface.GetRandomRoomEmptyCell(); //何もない部屋座標を取得
            var trap = DungeonProgressManager.Interface.GetRandomTrapSetup();
            var trapHolder = cell.GetInterface<ITrapHandler>();
            trapHolder.SetTrap(trap);
        }
    }
}