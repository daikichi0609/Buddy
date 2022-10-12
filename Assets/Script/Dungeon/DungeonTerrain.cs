using UnityEngine;
using System.Collections.Generic;
using UniRx;

/// <summary>
/// https://note.com/motibe_tsukuru/n/nbe75bb690bcc
/// </summary>

public class DungeonTerrain: SingletonMonoBehaviour<DungeonTerrain>
{
    private int[,] m_Map;
    public int[,] Map
    {
        get { return m_Map; }
        private set { m_Map = value; }
    }

    public void SetValueOnMap(int value, int x, int z)
    {
        Map[x, z] = value;
    }

    private List<List<GameObject>> m_TerrainList = new List<List<GameObject>>();
    public List<List<GameObject>> TerrainList
    {
        get { return m_TerrainList; }
        set { m_TerrainList = value; }
    }

    public GameObject GetTerrainListObject(int x, int z)
    {
        return TerrainList[x][z];
    }

    public void SetObjectInTerrainListInstead(GameObject @object, int x, int z)
    {
        GameObject removeObject = TerrainList[x][z];
        TerrainList[x].RemoveAt(z);
        Destroy(removeObject);
        TerrainList[x].Insert(z, @object);
    }

    private List<List<GameObject>> m_RoomList = new List<List<GameObject>>();
    public List<List<GameObject>> RoomList
    {
        get { return m_RoomList; }
        set { m_RoomList = value; }
    }

    //部屋IDの部屋オブジェクトリストを取得
    //-1に注意
    public List<GameObject> GetRoomList(int roomId)
    {
        return RoomList[roomId - 1];
    }

    public GameObject GetRoomListObject(int id, int num)
    {
        return RoomList[id][num];
    }

    [SerializeField] private List<Range> m_RangeList = new List<Range>();
    public List<Range> RangeList
    {
        get { return m_RangeList; }
        set { m_RangeList = value; }
    }

    private int m_MapSizeX = 32;
    private int m_MapSizeZ = 32;
    private int m_MaxRoom = 8;

    public enum GRID_ID
    {
        WALL = 0,
        PATH_WAY = 1,
        ROOM = 2,
        GATE = 3,
        STAIRS = 4,
        NONE = -1
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

    protected override void Awake()
    {
        base.Awake();

        GameManager.Instance.GetInit.Subscribe(_ => DeployDungeon());
    }

    public void DeployDungeon()
    {
        Map = MapGenerator.Instance.GenerateMap(m_MapSizeX, m_MapSizeZ, m_MaxRoom);

        DeployDungeonTerrain();
        DeployStairs();
        AddRoomObjectToList();
        RegisterRoomID();
    }

    public void RemoveDungeon()
    {
        foreach(List<GameObject> list in TerrainList)
        {
            foreach(GameObject terrain in list)
            {
                GRID_ID id = terrain.GetComponent<Grid>().GridID;
                string key = id.ToString();
                ObjectPool.Instance.SetObject(key, terrain);
            }
        }

        InitializeAllList();
    }

    private void InitializeAllList()
    {
        TerrainList = new List<List<GameObject>>();
        RoomList = new List<List<GameObject>>();
        RangeList = new List<Range>();
    }

    private void DeployDungeonTerrain()
    {
        for (int i = 0; i < Map.GetLength(0) - 1; i++)
        {
            TerrainList.Add(new List<GameObject>());

            for (int j = 0; j < Map.GetLength(1) - 1; j++)
            {
                int id = Map[i, j];
                GameObject obj;
                switch (id)
                {
                    case (int)GRID_ID.WALL: //0
                        obj = ObjectPool.Instance.PoolObject(GRID_ID.WALL.ToString());
                        if(obj == null)
                        {
                            obj = Instantiate(DungeonContentsHolder.Instance.Wall, new Vector3(i, 0, j), Quaternion.identity);
                            obj.GetComponent<Grid>().GridID = GRID_ID.WALL;
                        }
                        else
                        {
                            obj.transform.position = new Vector3(i, 0, j);
                        }
                        TerrainList[i].Add(obj);
                        break;

                    case (int)GRID_ID.PATH_WAY: //1
                        obj = ObjectPool.Instance.PoolObject(GRID_ID.PATH_WAY.ToString());
                        if (obj == null)
                        {
                            obj = Instantiate(PathWayGrid(), new Vector3(i, 0, j), Quaternion.identity);
                            obj.GetComponent<Grid>().GridID = GRID_ID.PATH_WAY;
                        }
                        else
                        {
                            obj.transform.position = new Vector3(i, 0, j);
                        }
                        TerrainList[i].Add(obj);
                        break;

                    case (int)GRID_ID.ROOM: //2
                        AroundGridID aroundGrid = CreateAroundGrid(i, j);
                        if (CheckGateWay(aroundGrid) == true)
                        {
                            Map[i, j] = (int)GRID_ID.GATE; //3
                            obj = ObjectPool.Instance.PoolObject(GRID_ID.GATE.ToString());
                            if (obj == null)
                            {
                                obj = Instantiate(RoomGrid(), new Vector3(i, 0, j), Quaternion.identity);
                                obj.GetComponent<Grid>().GridID = GRID_ID.GATE;
                            }
                            else
                            {
                                obj.transform.position = new Vector3(i, 0, j);
                            }
                            TerrainList[i].Add(obj);
                        }
                        else
                        {
                            obj = ObjectPool.Instance.PoolObject(GRID_ID.ROOM.ToString());
                            if (obj == null)
                            {
                                obj = Instantiate(RoomGrid(), new Vector3(i, 0, j), Quaternion.identity);
                                obj.GetComponent<Grid>().GridID = GRID_ID.ROOM;
                            }
                            else
                            {
                                obj.transform.position = new Vector3(i, 0, j);
                            }
                            TerrainList[i].Add(obj);
                        }
                        break;
                }
            }
        }
    }

    // 4 -> 階段
    private void DeployStairs() //階段配置
    {
        int[,] map = Map; //マップ取得
        int[] coord = DungeonContents.Instance.ChooseEmptyRandomRoomGrid(map); //何もない部屋座標を取得

        SetValueOnMap((int)GRID_ID.STAIRS, coord[0], coord[1]); //マップに階段を登録
        GameObject gridObject = ObjectPool.Instance.PoolObject(GRID_ID.STAIRS.ToString());
        if(gridObject == null)
        {
            gridObject = Instantiate(DungeonContentsHolder.Instance.Stairs, new Vector3(coord[0], 0, coord[1]), Quaternion.identity); //オブジェクト生成
            gridObject.GetComponent<Grid>().GridID = GRID_ID.STAIRS;
        }
        else
        {
            gridObject.transform.position = new Vector3(coord[0], 0, coord[1]);
        }
        
        Instance.SetObjectInTerrainListInstead(gridObject, coord[0], coord[1]); //既存のオブジェクトを破壊して代わりに代入
    }

    public void AddRoomObjectToList()
    {
        RoomList = new List<List<GameObject>>();
        foreach (Range range in RangeList)
        {
            List<GameObject> list = new List<GameObject>();

            for (int x = range.Start.X; x <= range.End.X; x++)
            {
                for(int z = range.Start.Y; z <= range.End.Y; z++)
                {
                    list.Add(GetTerrainListObject(x, z));
                }
            }
            RoomList.Add(list);
        }
    }

    public void RegisterRoomID()
    {
        for(int id = 0; id <= RoomList.Count - 1; id++)
        {
            for(int num = 0; num <= RoomList[id].Count - 1; num++)
            {
                Grid grid = GetRoomListObject(id, num).GetComponent<Grid>();
                grid.RoomID = id + 1;
            }
        }
    }

    private GameObject PathWayGrid()
    {
        switch(GameManager.Instance.DungeonTheme)
        {
            case Define.DUNGEON_THEME.GRASS:
                return DungeonContentsHolder.Instance.Grass_C;

            case Define.DUNGEON_THEME.ROCK:
                return DungeonContentsHolder.Instance.Rock_C;

            case Define.DUNGEON_THEME.WHITE:
                return DungeonContentsHolder.Instance.White_C;

            case Define.DUNGEON_THEME.CRYSTAL:
                return DungeonContentsHolder.Instance.CrystalRock_C;
        }
        return null;
    }

    private GameObject RoomGrid()
    {
        switch (GameManager.Instance.DungeonTheme)
        {
            case Define.DUNGEON_THEME.GRASS:
                return DungeonContentsHolder.Instance.Grass_A;

            case Define.DUNGEON_THEME.ROCK:
                return DungeonContentsHolder.Instance.Rock_A;

            case Define.DUNGEON_THEME.WHITE:
                return DungeonContentsHolder.Instance.White_A;

            case Define.DUNGEON_THEME.CRYSTAL:
                return DungeonContentsHolder.Instance.CrystalRock_A;
        }
        return null;
    }

    public AroundGridID CreateAroundGrid(int x, int z)
    {
        return new AroundGridID(Map, x, z);
    }

    private bool CheckGateWay(AroundGridID aroundGrid)
    {
        if (aroundGrid.UpGrid == (int)GRID_ID.PATH_WAY)
        {
            return true;
        }
        if (aroundGrid.UnderGrid == (int)GRID_ID.PATH_WAY)
        {
            return true;
        }
        if (aroundGrid.LeftGrid == (int)GRID_ID.PATH_WAY)
        {
            return true;
        }
        if (aroundGrid.RightGrid == (int)GRID_ID.PATH_WAY)
        {
            return true;
        }
        return false;
    }

    public int GridID(int pos_x, int pos_z)
    {
        return Map[pos_x, pos_z];
    }

    public int DestinationGridID(int pos_x, int pos_z, int direction_x, int direction_z)
    {
        return Map[pos_x + direction_x, pos_z + direction_z];
    }

    public bool IsPossibleToMoveDiagonal(int pos_x, int pos_z, int direction_x, int direction_z)
    {
        if(direction_x == 0 || direction_z == 0)
        {
            return true;
        }
        if (Map[pos_x + direction_x, pos_z] == (int)GRID_ID.WALL || Map[pos_x, pos_z + direction_z] == (int)GRID_ID.WALL)
        {
            return false;
        }
        return true;
    }
}

public struct AroundGridID
{
    public int UpGrid { get; } //上
    public int UnderGrid { get; } //下
    public int LeftGrid { get; } //左
    public int RightGrid { get; } //右

    public int UpperLeft { get; } //左上
    public int UpperRight { get; } //右上
    public int LowerLeft { get; } //左下
    public int LowerRight { get; } //右下

    public AroundGridID(int[,] map, int x, int z)
    {
        UpGrid = map[x, z + 1];
        UnderGrid = map[x, z - 1];
        LeftGrid = map[x - 1, z];
        RightGrid = map[x + 1, z];

        UpperLeft = map[x - 1, z + 1];
        UpperRight = map[x + 1, z + 1];
        LowerLeft = map[x - 1, z - 1];
        LowerRight = map[x + 1, z - 1];
    }
}