using UnityEngine;
using System.Collections.Generic;
using UniRx;
using Zenject;

public interface IDungeonHandler
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
    TERRAIN_ID GetCellId(Vector3Int pos);

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
    /// <param name="pos"></param>
    /// <returns></returns>
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
    /// -1 は不正
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    bool TryGetRoomId(Vector3Int pos, out int id);

    /// <summary>
    /// ランダムな何もないセルを返す
    /// </summary>
    /// <returns></returns>
    ICollector GetRandomRoomEmptyCell();

    /// <summary>
    /// ランダムな何もない部屋セルの座標を返す
    /// </summary>
    /// <returns></returns>
    Vector3Int GetRandomRoomEmptyCellPosition();

    /// <summary>
    /// 探索済みとしてマーク
    /// </summary>
    void MarkExplored(Vector3Int pos);
}

public class DungeonHandler : IDungeonHandler
{
    [Inject]
    private IDungeonDeployer m_DungeonDeployer;
    [Inject]
    private IDungeonHandler m_DungeonHandler;
    [Inject]
    private IItemManager m_ItemManager;
    [Inject]
    private IUnitFinder m_UnitFinder;

    /// <summary>
    /// マップ
    /// </summary>
    private TERRAIN_ID[,] IdMap => m_DungeonDeployer.IdMap;
    private List<List<ICollector>> CellMap => m_DungeonDeployer.CellMap;

    /// <summary>
    /// セル取得
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    private ICollector GetCell(Vector3Int pos) => CellMap[pos.x][pos.z];
    ICollector IDungeonHandler.GetCell(Vector3Int pos) => GetCell(pos);

    /// <summary>
    /// セルId取得
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    private TERRAIN_ID GetCellId(Vector3Int pos) => IdMap[pos.x, pos.z];
    TERRAIN_ID IDungeonHandler.GetCellId(Vector3Int pos) => GetCellId(pos);

    /// <summary>
    /// 周囲のセル取得
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    private AroundCell GetAroundCell(Vector3Int pos) => new AroundCell(CellMap, pos.x, pos.z);
    AroundCell IDungeonHandler.GetAroundCell(Vector3Int pos) => GetAroundCell(pos);

    /// <summary>
    /// 周囲のセルIDを取得
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    AroundCellId IDungeonHandler.GetAroundCellId(Vector3Int pos) => new AroundCellId(IdMap, pos.x, pos.z);

    /// <summary>
    /// 任意座標の部屋Id取得
    /// -1 は無効
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    private bool TryGetRoomId(Vector3Int pos, out int id)
    {
        var cell = CellMap[pos.x][pos.z];
        var info = cell.GetInterface<ICellInfoHandler>();
        id = info.RoomId;
        return id != -1;
    }
    bool IDungeonHandler.TryGetRoomId(Vector3Int pos, out int id) => TryGetRoomId(pos, out id);

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

        if (id == TERRAIN_ID.PATH_WAY || id == TERRAIN_ID.ROOM || id == TERRAIN_ID.GATE || id == TERRAIN_ID.STAIRS)
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
        if (IdMap[pos.x + dir.x, pos.z] == TERRAIN_ID.WALL || IdMap[pos.x, pos.z + dir.z] == TERRAIN_ID.WALL)
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
        var room = m_DungeonDeployer.GetRoom(roomId);
        List<ICollector> roomList = room.Cells;
        List<ICollector> list = new List<ICollector>();
        foreach (ICollector cell in roomList)
            if (cell.RequireInterface<ICellInfoHandler>(out var info) == true)
                if (info.CellId == TERRAIN_ID.GATE)
                    list.Add(cell);

#if DEBUG
        if (list.Count == 0)
            Debug.Log("ゲートセルが存在しない部屋です");
#endif

        return list;
    }

    /// <summary>
    /// 何も乗っていないかを判定
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    private bool IsNothingThere(Vector3Int pos)
    {
        // ユニット
        if (m_UnitFinder.IsUnitOn(pos) == true)
            return false;

        // アイテム
        if (m_ItemManager.IsItemOn(pos) == true)
            return false;

        // 罠
        var cell = m_DungeonHandler.GetCell(pos);
        if (cell.GetInterface<ITrapHandler>().HasTrap == true)
            return false;

        return true;
    }

    /// <summary>
    /// ランダムな部屋のセルを返す
    /// </summary>
    /// <returns></returns>
    private ICollector GetRandamRoomCell()
    {
        var room = m_DungeonDeployer.GetRandomRoom();
        return room.GetRandomCell();
    }

    /// <summary>
    /// ランダムな何もない部屋セルを返す
    /// </summary>
    /// <returns></returns>
    private ICollector GetRandomRoomEmptyCell()
    {
        ICollector cell = null;
        while (cell == null)
        {
            var temp = GetRandamRoomCell();
            if (temp.RequireInterface<ICellInfoHandler>(out var info) == false)
                continue;

            // GATEやSTAIRSの可能性があるのでチェック
            if (info.CellId != TERRAIN_ID.ROOM)
                continue;

            if (IsNothingThere(info.Position) == true)
                cell = temp;
        }
        return cell;
    }
    ICollector IDungeonHandler.GetRandomRoomEmptyCell() => GetRandomRoomEmptyCell();

    /// <summary>
    /// ランダムな何もないセルの座標を返す
    /// </summary>
    /// <returns></returns>
    private Vector3Int GetRandomRoomEmptyCellPosition()
    {
        // 初期化
        var cell = GetRandomRoomEmptyCell(); //何もない部屋座標を取得
        var info = cell.GetInterface<ICellInfoHandler>();
        return info.Position;
    }
    Vector3Int IDungeonHandler.GetRandomRoomEmptyCellPosition() => GetRandomRoomEmptyCellPosition();

    /// <summary>
    /// 探索済みとしてマーク
    /// </summary>
    /// <param name="pos"></param>
    void IDungeonHandler.MarkExplored(Vector3Int pos)
    {
        // 部屋の中にいるなら部屋全体を探索済みとする
        if (TryGetRoomId(pos, out var id) == true)
        {
            var room = m_DungeonDeployer.GetRoom(id);
            foreach (var cell in room.Cells)
                MarkExploredInternal(cell);
        }
        // 部屋にいないなら周囲のセルを探索済みとする
        else
        {
            var around = GetAroundCell(pos);
            foreach (var cell in around.Cells.Values)
                MarkExploredInternal(cell);
        }

        void MarkExploredInternal(ICollector cell)
        {
            var state = cell.GetInterface<ICellStateChanger>();
            state.IsExplored = true;
        }
    }
}