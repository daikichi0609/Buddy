using UnityEngine;
using System.Collections.Generic;
using UniRx;

public interface IDungeonHandler : ISingleton
{
    /// <summary>
    /// セル
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    ICollector GetCell(Vector3Int pos);

    /// <summary>
    /// セルId
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    CELL_ID GetCellId(Vector3Int pos);

    /// <summary>
    /// 周りのセル取得
    /// </summary>
    /// <param name="x"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    AroundCell GetAroundCell(Vector3Int pos);

    /// <summary>
    /// 周りのセルId取得
    /// </summary>
    /// <param name="x"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    AroundCellId GetAroundCellId(int x, int z);
    AroundCellId GetAroundCellId(Vector3Int pos);

    /// <summary>
    /// 任意の地点から任意の方向に移動できるか
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="dir"></param>
    /// <returns></returns>
    bool CanMove(Vector3Int pos, DIRECTION dir);

    /// <summary>
    /// 任意の部屋の入り口セル取得
    /// </summary>
    /// <param name="roomId"></param>
    /// <returns></returns>
    List<ICollector> GetGateWayCells(int roomId);

    /// <summary>
    /// 任意の位置の部屋Id取得
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    int GetRoomId(Vector3Int pos);

    /// <summary>
    /// ランダムな部屋のセルを取得
    /// </summary>
    /// <returns></returns>
    ICollector GetRandomRoomCell();

    /// <summary>
    /// ランダムな部屋の何もないセルを取得
    /// </summary>
    /// <returns></returns>
    ICollector GetRandomRoomEmptyCell();
}

public partial class DungeonHandler : Singleton<DungeonHandler, IDungeonHandler>, IDungeonHandler
{
    /// <summary>
    /// マップ
    /// </summary>
    private CELL_ID[,] IdMap => DungeonManager.Interface.IdMap;
    private List<List<ICollector>> CellMap => DungeonManager.Interface.CellMap;

    /// <summary>
    /// セル取得
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    private ICollector GetCell(Vector3Int pos) => DungeonManager.Interface.CellMap[pos.x][pos.z];
    ICollector IDungeonHandler.GetCell(Vector3Int pos) => GetCell(pos);

    /// <summary>
    /// セルId取得
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    private CELL_ID GetCellId(Vector3Int pos) => DungeonManager.Interface.IdMap[pos.x, pos.z];
    CELL_ID IDungeonHandler.GetCellId(Vector3Int pos) => GetCellId(pos);

    /// <summary>
    /// 周囲のセル取得
    /// </summary>
    /// <param name="x"></param>
    /// <param name="z"></param>
    /// <returns></returns>s
    private AroundCell NewAroundCell(int x, int z) => new AroundCell(CellMap, x, z);
    AroundCell IDungeonHandler.GetAroundCell(Vector3Int pos) => NewAroundCell(pos.x, pos.z);

    /// <summary>
    /// 周囲のセルId取得
    /// </summary>
    /// <param name="x"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    private AroundCellId NewAroundCellId(int x, int z) => new AroundCellId(IdMap, x, z);
    AroundCellId IDungeonHandler.GetAroundCellId(int x, int z) => NewAroundCellId(x, z);
    AroundCellId IDungeonHandler.GetAroundCellId(Vector3Int pos) => NewAroundCellId(pos.x, pos.z);

    /// <summary>
    /// 任意座標の部屋Id取得
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    int IDungeonHandler.GetRoomId(Vector3Int pos)
    {
        var cell = DungeonManager.Interface.CellMap[pos.x][pos.z];
        var info = cell.GetInterface<ICellInfoHolder>();
        return info.RoomId;
    }

    /// <summary>
    /// 移動可能か
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="direction"></param>
    /// <returns></returns>
    private bool CanMove(Vector3Int pos, DIRECTION dir) //指定座標から指定方向へ１マス移動可能かどうか調べる
    {
        var destinationPos = pos + dir.ToV3Int();
        if (IsPassable(destinationPos) == false)
            return false;

        if (CanMoveDiagonal(pos, dir.ToV3Int()) == false)
            return false;

        return true;
    }
    bool IDungeonHandler.CanMove(Vector3Int pos, DIRECTION dir) => CanMove(pos, dir);

    /// <summary>
    /// 通れるか
    /// </summary>
    /// <returns></returns>
    private bool IsPassable(Vector3Int pos)
    {
        var id = IdMap[pos.x, pos.z];

        if (id == CELL_ID.PATH_WAY || id == CELL_ID.ROOM || id == CELL_ID.GATE || id == CELL_ID.STAIRS)
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
    private bool CanMoveDiagonal(Vector3Int pos, Vector3Int dir)
    {
        if (IdMap[pos.x + dir.x, pos.z] == CELL_ID.WALL || IdMap[pos.x, pos.z + dir.z] == CELL_ID.WALL)
            return false;

        return true;
    }

    /// <summary>
    /// 特定の部屋の出入り口を取得
    /// </summary>
    /// <param name="roomId"></param>
    /// <returns></returns>
    List<ICollector> IDungeonHandler.GetGateWayCells(int roomId)
    {
        List<ICollector> roomList = DungeonManager.Interface.GetRoomCellList(roomId);
        List<ICollector> list = new List<ICollector>();
        foreach (ICollector cell in roomList)
            if (cell.RequireInterface<ICellInfoHolder>(out var info) == true)
                if (info.CellId == CELL_ID.GATE)
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
        if (UnitFinder.Interface.IsUnitOn(pos) == true)
            return false;

        // TODO:アイテム

        return true;
    }

    /// <summary>
    /// ランダムな部屋のセルを返す
    /// </summary>
    /// <returns></returns>
    private ICollector GetRandamRoomCell()
    {
        var id = CELL_ID.NONE;
        int x = -1;
        int z = -1;

        while (id != CELL_ID.ROOM)
        {
            x = Random.Range(0, IdMap.GetLength(0));
            z = Random.Range(0, IdMap.GetLength(1));
            id = IdMap[x, z];
        }

        return DungeonManager.Interface.CellMap[x][z];
    }
    ICollector IDungeonHandler.GetRandomRoomCell() => GetRandamRoomCell();

    /// <summary>
    /// ランダムな何も乗っていない部屋セルを返す
    /// </summary>
    /// <returns></returns>
    ICollector IDungeonHandler.GetRandomRoomEmptyCell()
    {
        ICollector cell = null;
        bool isEmpty = false;
        while (isEmpty == false)
        {
            var temp = GetRandamRoomCell();
            if (temp.RequireInterface<ICellInfoHolder>(out var info) == false)
                continue;

            if (IsNothingThere(info.Position) == true)
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
    public CELL_ID BaseCell { get; }
    public Dictionary<DIRECTION, CELL_ID> Cells { get; }

    public AroundCellId(CELL_ID[,] map, int x, int z)
    {
        BaseCell = map[x, z];
        Cells = new Dictionary<DIRECTION, CELL_ID>();

        // 左
        if (x - 1 >= 0 && z - 1 >= 0)
            Cells.Add(DIRECTION.LOWER_LEFT, map[x - 1, z - 1]);

        if (x - 1 >= 0)
            Cells.Add(DIRECTION.LEFT, map[x - 1, z]);

        if (x - 1 >= 0 && z + 1 < map.Length)
            Cells.Add(DIRECTION.UPPER_LEFT, map[x - 1, z + 1]);

        // 上
        if (z + 1 < map.Length)
            Cells.Add(DIRECTION.UP, map[x, z + 1]);

        if (x + 1 < map.Length && z + 1 < map.Length)
            Cells.Add(DIRECTION.UPPER_RIGHT, map[x + 1, z + 1]);

        // 右
        if (x + 1 < map.Length)
            Cells.Add(DIRECTION.RIGHT, map[x + 1, z]);

        if (x + 1 < map.Length && z - 1 >= 0)
            Cells.Add(DIRECTION.LOWER_RIGHT, map[x + 1, z - 1]);

        // 下
        if (z - 1 >= 0)
            Cells.Add(DIRECTION.UNDER, map[x, z - 1]);
    }
}

public readonly struct AroundCell
{
    public ICollector BaseCell { get; } //基準
    public Dictionary<DIRECTION, ICollector> Cells { get; }

    public AroundCell(List<List<ICollector>> cellList, int x, int z)
    {
        BaseCell = cellList[x][z];
        Cells = new Dictionary<DIRECTION, ICollector>();

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
        if (x + 1 < cellList.Count)
            Cells.Add(DIRECTION.RIGHT, cellList[x + 1][z]);

        if (x + 1 < cellList.Count && z - 1 >= 0)
            Cells.Add(DIRECTION.LOWER_RIGHT, cellList[x + 1][z - 1]);

        // 下
        if (z - 1 >= 0)
            Cells.Add(DIRECTION.UNDER, cellList[x][z - 1]);
    }
}