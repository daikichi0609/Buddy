using System.Collections.Generic;

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
    public static AroundCellId GetAroundCellId(this ICollector cell)
    {
        var pos = cell.GetInterface<ICellInfoHandler>();
        return DungeonHandler.Interface.GetAroundCellId(pos.Position);
    }

    /// <summary>
    /// 周囲のセルを取得
    /// </summary>
    /// <param name="cell"></param>
    /// <returns></returns>
    public static AroundCell GetAroundCell(this ICollector cell)
    {
        var pos = cell.GetInterface<ICellInfoHandler>();
        return DungeonHandler.Interface.GetAroundCell(pos.Position);
    }

    /// <summary>
    /// マップをAstarGridに変換
    /// </summary>
    /// <param name="map"></param>
    /// <returns></returns>
    public static int[,] AstarGrid(this TERRAIN_ID[,] map)
    {
        var xLength = map.GetLength(0);
        var zLength = map.GetLength(1);
        int[,] grid = new int[xLength, zLength];

        for (int x = 0; x < xLength; x++)
            for (int z = 0; z < zLength; z++)
            {
                var terrain = map[x, z];
                // Wallは0（通過できない）
                // Wall以外は全て1（通過できる）
                var i = terrain switch
                {
                    TERRAIN_ID.WALL => 0,
                    _ => 1
                };
                grid[x, z] = i;
            }

        return grid;
    }
}

/// <summary>
/// 周囲のセルId
/// </summary>
public readonly struct AroundCellId
{
    public TERRAIN_ID BaseCell { get; }
    public Dictionary<DIRECTION, TERRAIN_ID> Cells { get; }

    public AroundCellId(TERRAIN_ID[,] map, int x, int z)
    {
        BaseCell = map[x, z];
        Cells = new Dictionary<DIRECTION, TERRAIN_ID>();

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