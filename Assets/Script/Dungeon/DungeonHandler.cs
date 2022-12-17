using UnityEngine;
using System.Collections.Generic;
using UniRx;

public interface IDungeonHandler : ISingleton
{
    AroundCell GetAroundCell(int x, int z);
    AroundCellId GetAroundCellId(int x, int z);

    bool CanMoveDiagonal(Vector3 pos, Vector3 dir);
    bool CanMoveDiagonal(Vector3 pos, DIRECTION dir);

    List<ICell> GetGateWayCells(int roomId);

    int GetRoomId(Vector3 pos);

    ICell GetRandomRoomCell();
    ICell GetRandomRoomEmptyCell();
}

public partial class DungeonHandler : Singleton<DungeonHandler, IDungeonHandler>, IDungeonHandler
{
    private int[,] Map => DungeonManager.Interface.Map;
    private List<List<ICell>> CellMap => DungeonManager.Interface.CellMap;

    private AroundCell NewAroundCell(int x, int z) => new AroundCell(CellMap, x, z);
    AroundCell IDungeonHandler.GetAroundCell(int x, int z) => NewAroundCell(x, z);

    private AroundCellId NewAroundCellId(int x, int z) => new AroundCellId(Map, x, z);
    AroundCellId IDungeonHandler.GetAroundCellId(int x, int z) => NewAroundCellId(x, z);

    int IDungeonHandler.GetRoomId(Vector3 pos) => DungeonManager.Interface.CellMap[(int)pos.x][(int)pos.z].RoomId;

    /// <summary>
    /// 移動可能か
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="direction"></param>
    /// <returns></returns>
    private bool CanMove(Vector3 pos, DIRECTION dir) //指定座標から指定方向へ１マス移動可能かどうか調べる
    {
        int[,] map = Map;
        int direction_x = (int)Positional.GetDirection(dir).x;
        int direction_z = (int)Positional.GetDirection(dir).z;

        if (direction_x != 0 && direction_z != 0) //斜め移動の場合、壁が邪魔になっていないかどうかチェックする
            if (CanMoveDiagonal(pos, dir) == false)
                return false;

        int pos_x = (int)pos.x;
        int pos_z = (int)pos.z;

        int id = Map[pos_x + direction_x, pos_z + direction_z];

        if (id == (int)GRID_ID.PATH_WAY || id == (int)GRID_ID.ROOM || id == (int)GRID_ID.GATE || id == (int)GRID_ID.STAIRS)
            return true;

        return false;
    }

    /// <summary>
    /// 斜め移動できるかを調べる
    /// </summary>
    /// <param name="pos_x"></param>
    /// <param name="pos_z"></param>
    /// <param name="direction_x"></param>
    /// <param name="direction_z"></param>
    /// <returns></returns>
    private bool CanMoveDiagonal(Vector3 pos, DIRECTION direction) => CanMoveDiagonal(pos, Positional.GetDirection(direction));
    private bool CanMoveDiagonal(Vector3 pos, Vector3 dir)
    {
        if (Map[(int)(pos.x + dir.x), (int)pos.z] == (int)GRID_ID.WALL || Map[(int)pos.x, (int)(pos.z + dir.z)] == (int)GRID_ID.WALL)
            return false;

        return true;
    }

    bool IDungeonHandler.CanMoveDiagonal(Vector3 pos, Vector3 dir) => CanMoveDiagonal(pos, dir);
    bool IDungeonHandler.CanMoveDiagonal(Vector3 pos, DIRECTION dir) => CanMoveDiagonal(pos, dir);

    /// <summary>
    /// 特定の部屋の出入り口を取得
    /// </summary>
    /// <param name="roomId"></param>
    /// <returns></returns>
    List<ICell> IDungeonHandler.GetGateWayCells(int roomId)
    {
        List<ICell> roomList = DungeonManager.Interface.GetRoomCellList(roomId);
        List<ICell> list = new List<ICell>();
        foreach (ICell cell in roomList)
            if (cell.GridID == GRID_ID.GATE)
                list.Add(cell);

        return list;
    }

    /// <summary>
    /// 何も乗っていないかを判定
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    private bool IsNothingThere(Vector3 pos)
    {
        if (UnitManager.Interface.IsUnitOn(pos) == true)
            return false;

        // TODO:アイテム

        return true;
    }

    /// <summary>
    /// ランダムな部屋のセルを返す
    /// </summary>
    /// <returns></returns>
    private ICell GetRandamRoomCell()
    {
        int id = -1;
        int x = -1;
        int z = -1;

        while (id != 2)
        {
            x = Random.Range(0, Map.GetLength(0));
            z = Random.Range(0, Map.GetLength(1));
            id = Map[x, z];
        }

        return DungeonManager.Interface.CellMap[x][z];
    }
    ICell IDungeonHandler.GetRandomRoomCell() => GetRandamRoomCell();

    /// <summary>
    /// ランダムな何も乗っていない部屋セルを返す
    /// </summary>
    /// <returns></returns>
    ICell IDungeonHandler.GetRandomRoomEmptyCell()
    {
        ICell cell = null;
        bool isEmpty = false;
        while (isEmpty == false)
        {
            var temp = GetRandamRoomCell();
            if (IsNothingThere(temp.Position) == true)
            {
                isEmpty = true;
                cell = temp;
            }
        }
        return cell;
    }
}

/// <summary>
/// 周囲のセルId
/// </summary>
public readonly struct AroundCellId
{
    public int BaseCell { get; }
    public Dictionary<DIRECTION, int> Cells { get; }

    public AroundCellId(int[,] map, int x, int z)
    {
        BaseCell = map[x, z];
        Cells = new Dictionary<DIRECTION, int>();

        // 左
        if (x - 1 >= 0)
        {
            Cells.Add(DIRECTION.LOWER_LEFT, map[x - 1, z - 1]);
            Cells.Add(DIRECTION.LEFT, map[x - 1, z]);
            Cells.Add(DIRECTION.UPPER_LEFT, map[x - 1, z + 1]);
        }

        // 上
        if (z + 1 < map.Length)
        {
            Cells.Add(DIRECTION.UP, map[x, z + 1]);
            Cells.Add(DIRECTION.UPPER_RIGHT, map[x + 1, z + 1]);
        }

        // 右
        if (x + 1 < map.Length)
        {
            Cells.Add(DIRECTION.RIGHT, map[x + 1, z]);
            Cells.Add(DIRECTION.LOWER_RIGHT, map[x + 1, z - 1]);
        }

        // 下
        if (z - 1 >= 0)
            Cells.Add(DIRECTION.UNDER, map[x, z - 1]);
    }
}

public readonly struct AroundCell
{
    public ICell BaseCell { get; } //基準
    public Dictionary<DIRECTION, ICell> Cells { get; }

    public AroundCell(List<List<ICell>> cellList, int x, int z)
    {
        BaseCell = cellList[x][z];
        Cells = new Dictionary<DIRECTION, ICell>();

        // 左
        if (x - 1 >= 0 && z - 1 >= 0)
            Cells.Add(DIRECTION.LOWER_LEFT, cellList[x - 1][z - 1]);

        if (x - 1 >= 0)
            Cells.Add(DIRECTION.LEFT, cellList[x - 1][z]);

        if (x - 1 >= 0 && z + 1 < cellList[0].Count)
            Cells.Add(DIRECTION.UPPER_LEFT, cellList[x - 1][z + 1]);

        // 上
        if (z + 1 < cellList[0].Count)
            Cells.Add(DIRECTION.UP, cellList[x][z + 1]);

        if (x + 1 < cellList.Count && z + 1 < cellList[0].Count)
            Cells.Add(DIRECTION.UPPER_RIGHT, cellList[x + 1][z + 1]);

        // 右
        if(x + 1 < cellList.Count)
            Cells.Add(DIRECTION.RIGHT, cellList[x + 1][z]);

        if (x + 1 < cellList.Count && z - 1 >= 0)
            Cells.Add(DIRECTION.LOWER_RIGHT, cellList[x + 1][z - 1]);

        // 下
        if (z - 1 >= 0)
            Cells.Add(DIRECTION.UNDER, cellList[x][z - 1]);
    }
}