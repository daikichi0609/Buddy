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
    CELL_ID[,] IdMap { get; }

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
    void DeployDungeon();

    /// <summary>
    /// ダンジョンリムーブ
    /// </summary>
    void RemoveDungeon();

    /// <summary>
    /// ダンジョンデプロイ
    /// </summary>
    /// <param name="map"></param>
    /// <param name="range"></param>
    void DeployDungeon(CELL_ID[,] map, Range range);
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

public enum CELL_ID
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
    private CELL_ID[,] m_IdMap;
    CELL_ID[,] IDungeonDeployer.IdMap => m_IdMap;

    private void OverWriteCellId(CELL_ID value, int x, int z) => m_IdMap[x, z] = value;

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
                    var info = cell.GetInterface<ICellInfoHolder>();
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
    private void DeployDungeon()
    {
        var x = DungeonProgressManager.Interface.CurrentDungeonSetup.MapSize.x;
        var y = DungeonProgressManager.Interface.CurrentDungeonSetup.MapSize.y;
        var roomCount = DungeonProgressManager.Interface.CurrentDungeonSetup.RoomCountMax;
        var mapInfo = MapGenerator.GenerateMap(x, y, roomCount);
        m_IdMap = mapInfo.Map;
        Debug.Log("Map Reload");

        DeployDungeonTerrain();
        CreateRoomCellList(mapInfo.RangeList);
        DeployStairs();
    }
    void IDungeonDeployer.DeployDungeon() => DeployDungeon();

    /// <summary>
    /// ダンジョン配備（マップ指定）
    /// </summary>
    /// <param name="map"></param>
    /// <param name="range"></param>
    void IDungeonDeployer.DeployDungeon(CELL_ID[,] map, Range range)
    {
        m_IdMap = map;
        var rangeList = new List<Range>();
        rangeList.Add(range);
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
    private void DeployDungeonTerrain()
    {
        for (int i = 0; i < m_IdMap.GetLength(0) - 1; i++)
        {
            m_CellMap.Add(new List<ICollector>());

            for (int j = 0; j < m_IdMap.GetLength(1) - 1; j++)
            {
                var id = m_IdMap[i, j]; // 古いId
                CELL_ID type = CELL_ID.INVALID; // 新Id
                GameObject cellObject = null; // GameObject
                Vector3Int pos = new Vector3Int(i, 0, j);

                switch (id)
                {
                    case CELL_ID.WALL: // 0
                        if (ObjectPoolController.Interface.TryGetObject(CELL_ID.WALL.ToString(), out cellObject) == false)
                            cellObject = Instantiate(DungeonProgressManager.Interface.CurrentElementSetup.Wall, pos, Quaternion.identity);
                        else
                            cellObject.transform.position = pos;

                        type = CELL_ID.WALL;
                        break;

                    case CELL_ID.PATH_WAY: // 1
                        if (ObjectPoolController.Interface.TryGetObject(CELL_ID.PATH_WAY.ToString(), out cellObject) == false)
                            cellObject = Instantiate(DungeonProgressManager.Interface.CurrentElementSetup.Path, pos, Quaternion.identity);
                        else
                            cellObject.transform.position = pos;

                        type = CELL_ID.PATH_WAY;
                        break;

                    case CELL_ID.ROOM: // 2
                        AroundCellId aroundId = DungeonHandler.Interface.GetAroundCellId(i, j);
                        if (CheckGateWay(aroundId) == true)
                        {
                            m_IdMap[i, j] = CELL_ID.GATE; // 入口なら設定し直す

                            if (ObjectPoolController.Interface.TryGetObject(CELL_ID.GATE.ToString(), out cellObject) == false)
                                cellObject = Instantiate(DungeonProgressManager.Interface.CurrentElementSetup.Room, pos, Quaternion.identity);
                            else
                                cellObject.transform.position = pos;

                            type = CELL_ID.GATE;
                        }
                        else
                        {
                            if (ObjectPoolController.Interface.TryGetObject(CELL_ID.ROOM.ToString(), out cellObject) == false)
                                cellObject = Instantiate(DungeonProgressManager.Interface.CurrentElementSetup.Room, pos, Quaternion.identity);
                            else
                                cellObject.transform.position = pos;

                            type = CELL_ID.ROOM;
                        }
                        break;
                }

                var cell = cellObject.GetComponent<ICollector>();
                var info = cell.GetInterface<ICellInfoHolder>();
                info.CellObject = cellObject;
                info.CellId = type;
                m_CellMap[i].Add(cell);

                // 部屋なら罠抽選
                if (info.CellId == CELL_ID.ROOM)
                {
                    var prob = UnityEngine.Random.Range(0f, 1f);
                    if (prob <= DungeonProgressManager.Interface.CurrentDungeonSetup.TrapProb)
                        DeployTrap(cell, pos);
                }
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

            if (cells[DIRECTION.UP] == CELL_ID.PATH_WAY)
                return true;

            if (cells[DIRECTION.UNDER] == CELL_ID.PATH_WAY)
                return true;

            if (cells[DIRECTION.LEFT] == CELL_ID.PATH_WAY)
                return true;

            if (cells[DIRECTION.RIGHT] == CELL_ID.PATH_WAY)
                return true;

            return false;
        }
    }

    /// <summary>
    /// 4 -> 階段
    /// </summary>
    private void DeployStairs() //階段配置
    {
        var pos = DungeonHandler.Interface.GetRandomRoomEmptyCellPosition(); // 何もない部屋座標を取得
        var x = pos.x;
        var z = pos.z;

        OverWriteCellId(CELL_ID.STAIRS, x, z); // マップに階段を登録

        if (ObjectPoolController.Interface.TryGetObject(CELL_ID.STAIRS.ToString(), out var cellObject) == false)
            cellObject = Instantiate(DungeonProgressManager.Interface.CurrentElementSetup.Stairs, new Vector3(x, 0, z), Quaternion.identity); //オブジェクト生成
        else
            cellObject.transform.position = new Vector3(x, 0, z);

        var cell = cellObject.GetComponent<ICollector>();
        var info = cell.GetInterface<ICellInfoHolder>();
        info.CellObject = cellObject;
        info.CellId = CELL_ID.STAIRS;
        SetCellInstead(cell, x, z); // 既存のオブジェクトの代わりに代入
    }

    /// <summary>
    /// トラップ
    /// </summary>
    private void DeployTrap(ICollector cell, Vector3Int pos)
    {
        var trap = DungeonProgressManager.Interface.GetRandomTrapSetup();
        var trapHolder = cell.GetInterface<ITrapHandler>();
        trapHolder.SetTrap(trap, pos);
    }
}