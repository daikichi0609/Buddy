using UnityEngine;
using System.Collections.Generic;
using UniRx;

/// <summary>
/// https://note.com/motibe_tsukuru/n/nbe75bb690bcc
/// </summary>

public interface IDungeonManager : ISingleton
{
    int[,] Map { get; }
    void SetMapValue(int value, int x, int z);

    List<List<ICell>> CellMap { get; }

    List<ICell> GetRoomCellList(int roomId);

    List<Range> RangeList { get; }
}

public enum GRID_ID
{
    NONE = -1,
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
public class DungeonManager: Singleton<DungeonManager, IDungeonManager>, IDungeonManager
{
    /// <summary>
    /// マップ
    /// </summary>
    private int[,] m_Map;
    int[,] IDungeonManager.Map => m_Map;
    /*
    private int GetId(int x, int z)
    {
        return m_Map[x, z];
    }
    int IDungeonManager.GetId(int x, int z) => GetId(x, z);
    */
    private void SetMapValue(int value, int x, int z) => m_Map[x, z] = value;
    void IDungeonManager.SetMapValue(int value, int x, int z) => SetMapValue(value, x, z);

    private MapCreateSetting m_Setting = new MapCreateSetting(32, 32, 8);

    /// <summary>
    /// インスタンス
    /// </summary>
    private List<List<ICell>> m_CellMap = new List<List<ICell>>();
    List<List<ICell>> IDungeonManager.CellMap => m_CellMap;

    private void SetCell(ICell cell, int x, int z)
    {
        var remove = m_CellMap[x][z];
        m_CellMap[x].RemoveAt(z);
        Destroy(remove.GameObject);
        m_CellMap[x].Insert(z, cell);
    }

    /// <summary>
    /// インスタンス（ルーム限定）
    /// </summary>
    private List<List<ICell>> m_RoomCellList = new List<List<ICell>>();

    /// <summary>
    /// 部屋IDの部屋オブジェクトリストを取得
    /// -1に注意
    /// </summary>
    /// <param name="roomId"></param>
    /// <returns></returns>
    private List<ICell> GetRoomCellList(int roomId) => m_RoomCellList[roomId - 1];
    List<ICell> IDungeonManager.GetRoomCellList(int roomId) => GetRoomCellList(roomId);

    private ICell GetRoomCell(int id, int num) => m_RoomCellList[id][num];

    /// <summary>
    /// <see cref="m_RoomCellList"/> 初期化
    /// </summary>
    private void CreateRoomCellList()
    {
        m_RoomCellList = new List<List<ICell>>();
        foreach (Range range in m_RangeList)
        {
            List<ICell> list = new List<ICell>();

            for (int x = range.Start.X; x <= range.End.X; x++)
                for (int z = range.Start.Y; z <= range.End.Y; z++)
                    list.Add(m_CellMap[x][z]);

            m_RoomCellList.Add(list);
        }
    }

    /// <summary>
    /// 消したいかも
    /// </summary>
    private List<Range> m_RangeList = new List<Range>();
    List<Range> IDungeonManager.RangeList => m_RangeList;

    protected override void Awake()
    {
        base.Awake();

        GameManager.Interface.GetInitEvent.Subscribe(_ => DeployDungeon());
    }

    public void DeployDungeon()
    {
        m_Map = MapGenerator.Instance.GenerateMap(m_Setting.MapSizeX, m_Setting.MapSizeZ, m_Setting.MaxRoomCount);

        DeployDungeonTerrain();
        DeployStairs();
        CreateRoomCellList();
        RegisterRoomID();
    }

    public void RemoveDungeon()
    {
        foreach(List<ICell> list in m_CellMap)
        {
            foreach(ICell cell in list)
            {
                GRID_ID id = cell.GridID;
                string key = id.ToString();
                ObjectPool.Instance.SetObject(key, cell.GameObject);
            }
        }

        InitializeAllList();
    }

    private void InitializeAllList()
    {
        m_CellMap = new List<List<ICell>>();
        m_RoomCellList = new List<List<ICell>>();
        m_RangeList = new List<Range>();
    }

    private void DeployDungeonTerrain()
    {
        for (int i = 0; i < m_Map.GetLength(0) - 1; i++)
        {
            m_CellMap.Add(new List<ICell>());

            for (int j = 0; j < m_Map.GetLength(1) - 1; j++)
            {
                int id = m_Map[i, j];
                GameObject cellObject = null;
                GRID_ID type = GRID_ID.NONE;

                switch (id)
                {
                    case (int)GRID_ID.WALL: //0
                        if (ObjectPool.Instance.TryGetPoolObject(GRID_ID.WALL.ToString(), out cellObject) == false)
                            cellObject = Instantiate(DungeonContentsHolder.Instance.Wall, new Vector3(i, 0, j), Quaternion.identity);
                        else
                            cellObject.transform.position = new Vector3(i, 0, j);

                        type = GRID_ID.WALL;
                        break;

                    case (int)GRID_ID.PATH_WAY: //1
                        if (ObjectPool.Instance.TryGetPoolObject(GRID_ID.PATH_WAY.ToString(), out cellObject) == false)
                            cellObject = Instantiate(DungeonContentsHolder.Instance.PathWayGrid, new Vector3(i, 0, j), Quaternion.identity);
                        else
                            cellObject.transform.position = new Vector3(i, 0, j);

                        type = GRID_ID.PATH_WAY;
                        break;

                    case (int)GRID_ID.ROOM: //2
                        AroundCellId aroundId = DungeonHandler.Interface.GetAroundCellId(i, j);
                        if (CheckGateWay(aroundId) == true)
                        {
                            m_Map[i, j] = (int)GRID_ID.GATE; // 入口なら設定し直す

                            if (ObjectPool.Instance.TryGetPoolObject(GRID_ID.GATE.ToString(), out cellObject) == false)
                                cellObject = Instantiate(DungeonContentsHolder.Instance.RoomGrid, new Vector3(i, 0, j), Quaternion.identity);
                            else
                                cellObject.transform.position = new Vector3(i, 0, j);

                            type = GRID_ID.GATE;
                        }
                        else
                        {
                            if (ObjectPool.Instance.TryGetPoolObject(GRID_ID.ROOM.ToString(), out cellObject) == false)
                                cellObject = Instantiate(DungeonContentsHolder.Instance.RoomGrid, new Vector3(i, 0, j), Quaternion.identity);
                            else
                                cellObject.transform.position = new Vector3(i, 0, j);


                            type = GRID_ID.ROOM;
                        }
                        break;
                }

                var cell = cellObject.GetComponent<ICell>();
                cell.GameObject = cellObject;
                cell.GridID = type;
                m_CellMap[i].Add(cell);
            }
        }
    }

    // 4 -> 階段
    private void DeployStairs() //階段配置
    {
        var emp = DungeonHandler.Interface.GetRandomRoomEmptyCell(); //何もない部屋座標を取得
        var x = (int)emp.Position.x;
        var z = (int)emp.Position.z;

        SetMapValue((int)GRID_ID.STAIRS, x, z); //マップに階段を登録

        if (ObjectPool.Instance.TryGetPoolObject(GRID_ID.STAIRS.ToString(), out var cellObject) == false)
            cellObject = Instantiate(DungeonContentsHolder.Instance.Stairs, new Vector3(x, 0, z), Quaternion.identity); //オブジェクト生成
        else
            cellObject.transform.position = new Vector3(x, 0, z);

        var cell = cellObject.GetComponent<ICell>();
        cell.GridID = GRID_ID.STAIRS;
        SetCell(cell, x, z); //既存のオブジェクトの代わりに代入
    }

    /// <summary>
    /// 部屋Id登録
    /// </summary>
    public void RegisterRoomID()
    {
        for(int id = 0; id < m_RoomCellList.Count; id++)
        {
            for(int num = 0; num < m_RoomCellList[id].Count; num++)
            {
                ICell cell = GetRoomCell(id, num);
                cell.RoomId = id + 1;
            }
        }
    }

    /// <summary>
    /// 部屋の入り口になっているかどうかを調べる
    /// </summary>
    /// <param name="aroundGrid"></param>
    /// <returns></returns>
    private bool CheckGateWay(AroundCellId aroundGrid)
    {
        var cells = aroundGrid.Cells;

        if (cells[DIRECTION.UP] == (int)GRID_ID.PATH_WAY)
            return true;

        if (cells[DIRECTION.UNDER] == (int)GRID_ID.PATH_WAY)
            return true;

        if (cells[DIRECTION.LEFT] == (int)GRID_ID.PATH_WAY)
            return true;

        if (cells[DIRECTION.RIGHT] == (int)GRID_ID.PATH_WAY)
            return true;

        return false;
    }
}

/// <summary>
/// マップ作成に必要な設定
/// </summary>
public struct MapCreateSetting
{
    public MapCreateSetting(int x, int z, int roomCount)
    {
        MapSizeX = x;
        MapSizeZ = z;
        MaxRoomCount = roomCount;
    }

    public int MapSizeX { get; }
    public int MapSizeZ { get; }
    public int MaxRoomCount { get; }
}