
/// <summary>
/// セル拡張メソッド
/// </summary>
public static class CellExtension
{
    public static AroundCellId GetAroundCellId(this ICollector cell)
    {
        var pos = cell.GetInterface<ICellInfoHolder>();
        return DungeonHandler.Interface.GetAroundCellId(pos.Position);
    }

    public static AroundCell GetAroundCell(this ICollector cell)
    {
        var pos = cell.GetInterface<ICellInfoHolder>();
        return DungeonHandler.Interface.GetAroundCell(pos.Position);
    }
}

