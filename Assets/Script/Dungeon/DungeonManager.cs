using UnityEngine;
using System.Collections.Generic;
using UniRx;

/// <summary>
/// https://note.com/motibe_tsukuru/n/nbe75bb690bcc
/// </summary>

public interface IDungeonManager : ISingleton
{
    CELL_ID[,] IdMap { get; }
    List<List<ICollector>> CellMap { get; }

    List<ICollector> GetRoomCellList(int roomId);

    List<Range> RangeList { get; }

    void RemoveDungeon();
    void DeployDungeon();
}

public enum CELL_ID
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
public class DungeonManager : Singleton<DungeonManager, IDungeonManager>, IDungeonManager
{
    [SerializeField]
    private float m_TrapProb;

    /// <summary>
    /// マップ
    /// </summary>
    private CELL_ID[,] m_IdMap;
    CELL_ID[,] IDungeonManager.IdMap => m_IdMap;

    private void OverWriteCellId(CELL_ID value, int x, int z) => m_IdMap[x, z] = value;

    private MapCreateSetting m_Setting = new MapCreateSetting(32, 32, 8);

    /// <summary>
    /// インスタンス
    /// </summary>
    private List<List<ICollector>> m_CellMap = new List<List<ICollector>>();
    List<List<ICollector>> IDungeonManager.CellMap => m_CellMap;

    private void SetCell(ICollector cell, int x, int z)
    {
        var remove = m_CellMap[x][z];
        m_CellMap[x].RemoveAt(z);
        var info = remove.GetInterface<ICellInfoHolder>();
        Destroy(info.CellObject);
        m_CellMap[x].Insert(z, cell);
    }

    /// <summary>
    /// インスタンス（ルーム限定）
    /// </summary>
    private List<List<ICollector>> m_RoomCellList = new List<List<ICollector>>();

    /// <summary>
    /// 部屋IDの部屋オブジェクトリストを取得
    /// -1に注意
    /// </summary>
    /// <param name="roomId"></param>
    /// <returns></returns>
    private List<ICollector> GetRoomCellList(int roomId) => m_RoomCellList[roomId - 1];
    List<ICollector> IDungeonManager.GetRoomCellList(int roomId) => GetRoomCellList(roomId);

    private ICollector GetRoomCell(int id, int num) => m_RoomCellList[id][num];

    /// <summary>
    /// <see cref="m_RoomCellList"/> 初期化
    /// </summary>
    private void CreateRoomCellList()
    {
        m_RoomCellList = new List<List<ICollector>>();
        foreach (Range range in m_RangeList)
        {
            var list = new List<ICollector>();

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
        GameManager.Interface.GetInitEvent.Subscribe(_ => DeployDungeon()).AddTo(this);
    }

    private void DeployDungeon()
    {
        m_IdMap = MapGenerator.Instance.GenerateMap(m_Setting.MapSizeX, m_Setting.MapSizeZ, m_Setting.MaxRoomCount);
        Debug.Log("Map Reload");

        DeployDungeonTerrain();
        DeployStairs();
        CreateRoomCellList();
        RegisterRoomID();
    }
    void IDungeonManager.DeployDungeon() => DeployDungeon();

    public void RemoveDungeon()
    {
        foreach (var list in m_CellMap)
        {
            foreach (var cell in list)
            {
                var info = cell.GetInterface<ICellInfoHolder>();
                CELL_ID id = info.CellId;
                string key = id.ToString();
                ObjectPool.Instance.SetObject(key, info.CellObject);
            }
        }

        InitializeAllList();
    }
    void IDungeonManager.RemoveDungeon() => RemoveDungeon();

    private void InitializeAllList()
    {
        m_CellMap.Clear();
        m_RoomCellList.Clear();
        m_RangeList.Clear();
    }

    private void DeployDungeonTerrain()
    {
        for (int i = 0; i < m_IdMap.GetLength(0) - 1; i++)
        {
            m_CellMap.Add(new List<ICollector>());

            for (int j = 0; j < m_IdMap.GetLength(1) - 1; j++)
            {
                var id = m_IdMap[i, j];
                GameObject cellObject = null;
                CELL_ID type = CELL_ID.NONE;

                switch (id)
                {
                    case CELL_ID.WALL: // 0
                        if (ObjectPool.Instance.TryGetPoolObject(CELL_ID.WALL.ToString(), out cellObject) == false)
                            cellObject = Instantiate(DungeonContentsHolder.Instance.Wall, new Vector3(i, 0, j), Quaternion.identity);
                        else
                            cellObject.transform.position = new Vector3Int(i, 0, j);

                        type = CELL_ID.WALL;
                        break;

                    case CELL_ID.PATH_WAY: // 1
                        if (ObjectPool.Instance.TryGetPoolObject(CELL_ID.PATH_WAY.ToString(), out cellObject) == false)
                            cellObject = Instantiate(DungeonContentsHolder.Instance.PathWayGrid, new Vector3(i, 0, j), Quaternion.identity);
                        else
                            cellObject.transform.position = new Vector3Int(i, 0, j);

                        type = CELL_ID.PATH_WAY;
                        break;

                    case CELL_ID.ROOM: // 2
                        AroundCellId aroundId = DungeonHandler.Interface.GetAroundCellId(i, j);
                        if (CheckGateWay(aroundId) == true)
                        {
                            m_IdMap[i, j] = CELL_ID.GATE; // 入口なら設定し直す

                            if (ObjectPool.Instance.TryGetPoolObject(CELL_ID.GATE.ToString(), out cellObject) == false)
                                cellObject = Instantiate(DungeonContentsHolder.Instance.RoomGrid, new Vector3(i, 0, j), Quaternion.identity);
                            else
                                cellObject.transform.position = new Vector3Int(i, 0, j);

                            type = CELL_ID.GATE;
                        }
                        else
                        {
                            if (ObjectPool.Instance.TryGetPoolObject(CELL_ID.ROOM.ToString(), out cellObject) == false)
                                cellObject = Instantiate(DungeonContentsHolder.Instance.RoomGrid, new Vector3(i, 0, j), Quaternion.identity);
                            else
                                cellObject.transform.position = new Vector3Int(i, 0, j);

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
                    var prob = Random.Range(0f, 1f);
                    if (prob >= m_TrapProb)
                        DeployTrap(cell);
                }
            }
        }
    }

    /// <summary>
    /// 4 -> 階段
    /// </summary>
    private void DeployStairs() //階段配置
    {
        var emp = DungeonHandler.Interface.GetRandomRoomEmptyCell(); // 何もない部屋座標を取得
        var info = emp.GetInterface<ICellInfoHolder>();
        var x = info.Position.x;
        var z = info.Position.z;

        OverWriteCellId(CELL_ID.STAIRS, x, z); // マップに階段を登録

        if (ObjectPool.Instance.TryGetPoolObject(CELL_ID.STAIRS.ToString(), out var cellObject) == false)
            cellObject = Instantiate(DungeonContentsHolder.Instance.Stairs, new Vector3(x, 0, z), Quaternion.identity); //オブジェクト生成
        else
            cellObject.transform.position = new Vector3(x, 0, z);

        var cell = cellObject.GetComponent<ICollector>();
        info = cell.GetInterface<ICellInfoHolder>();
        info.CellObject = cellObject;
        info.CellId = CELL_ID.STAIRS;
        SetCell(cell, x, z); // 既存のオブジェクトの代わりに代入
    }

    /// <summary>
    /// トラップ
    /// </summary>
    private void DeployTrap(ICollector cell)
    {
        var trap = GetRandomTrap();
        var trapHolder = cell.GetInterface<ITrapHolder>();
        trapHolder.SetTrap(trap);
    }

    /// <summary>
    /// ランダムな罠インスタンスを返す
    /// </summary>
    /// <returns></returns>
    private ITrap GetRandomTrap()
    {
        return new BombTrap();
    }


    /// <summary>
    /// 部屋Id登録
    /// </summary>
    public void RegisterRoomID()
    {
        for (int id = 0; id < m_RoomCellList.Count; id++)
        {
            for (int num = 0; num < m_RoomCellList[id].Count; num++)
            {
                var cell = GetRoomCell(id, num);
                var info = cell.GetInterface<ICellInfoHolder>();
                info.RoomId = id + 1;
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