using System.Collections.Generic;
using Fungus;
using UnityEngine;

/// <summary>
/// セル拡張メソッド
/// </summary>
public static class CellExtension
{
    /// <summary>
    /// 周囲の地形Idを取得
    /// </summary>
    /// <param name="cell"></param>
    /// <returns></returns>
    public static AroundCell<TERRAIN_ID> GetAroundCellId(this TERRAIN_ID[,] map, int x, int z) => new AroundCell<TERRAIN_ID>(map, x, z);

    /// <summary>
    /// 周囲の地形Idを取得
    /// </summary>
    /// <param name="cell"></param>
    /// <returns></returns>
    public static AroundCell<TERRAIN_ID> GetAroundCellId(this ICollector cell, IDungeonHandler dungeonHandler)
    {
        var pos = cell.GetInterface<ICellInfoHandler>();
        return dungeonHandler.GetAroundCellId(pos.Position);
    }

    /// <summary>
    /// 周囲のセルを取得
    /// </summary>
    /// <param name="cell"></param>
    /// <returns></returns>
    public static AroundCell<ICollector> GetAroundCell(this ICollector cell, IDungeonHandler dungeonHandler)
    {
        var pos = cell.GetInterface<ICellInfoHandler>();
        return dungeonHandler.GetAroundCell(pos.Position);
    }

    /// <summary>
    /// マップをAstarGridに変換
    /// </summary>
    /// <param name="map"></param>
    /// <returns></returns>
    public static int[,] AstarGrid(this ICollector[,] map)
    {
        var xLength = map.GetLength(0);
        var zLength = map.GetLength(1);
        int[,] grid = new int[xLength, zLength];

        for (int x = 0; x < xLength; x++)
            for (int z = 0; z < zLength; z++)
            {
                var cell = map[x, z];
                var terrain = cell.GetInterface<ICellInfoHandler>().CellId;

                // Wallは0（通過できない）
                // Wall以外は全て1（通過できる）
                var cost = terrain switch
                {
                    TERRAIN_ID.WALL => 0,
                    _ => 1
                };

                if (cell.RequireInterface<ITrapHandler>(out var trap) == true && trap.IsVisible == true)
                    cost += 10;

                grid[x, z] = cost;
            }

        return grid;
    }
}

/// <summary>
/// 周囲のセルを取得
/// </summary>
public readonly struct AroundCell<T>
{
    public T BaseCell { get; }
    public Dictionary<DIRECTION, T> Cells { get; }

    public AroundCell(T[,] map, int centerX, int centerZ)
    {
        BaseCell = map[centerX, centerZ];
        Cells = new Dictionary<DIRECTION, T>();

        int xLength = map.GetLength(0);
        int zLength = map.GetLength(1);

        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                if (x == 0 && z == 0)
                    continue;

                int checkX = centerX + x;
                int checkZ = centerZ + z;

                if (checkX >= 0 && checkX < map.GetLength(0) && checkZ >= 0 && checkZ < map.GetLength(1))
                {
                    var dir = Positional.GetDirection(x, z);
                    Cells.Add(dir, map[checkX, checkZ]);
                }
            }
        }
    }
}