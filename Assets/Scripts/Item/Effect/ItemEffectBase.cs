using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public interface IItemEffect
{
    /// <summary>
    /// アイテムを食べる
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="item"></param>
    /// <param name="inventory"></param>
    /// <param name="itemManager"></param>
    /// <param name="dungeonHandler"></param>
    /// <param name="unitFinder"></param>
    /// <param name="battleLogManager"></param>
    /// <returns></returns>
    Task Eat(ICollector owner, ItemSetup item, ITeamInventory inventory, IItemManager itemManager, IDungeonHandler dungeonHandler, IUnitFinder unitFinder, IBattleLogManager battleLogManager);

    /// <summary>
    /// アイテムを投げる
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="item"></param>
    /// <param name="inventory"></param>
    /// <param name="itemManager"></param>
    /// <param name="dungeonHandler"></param>
    /// <param name="unitFinder"></param>
    /// <param name="battleLogManager"></param>
    /// <returns></returns>
    Task ThrowStraight(ICollector owner, ItemSetup item, ITeamInventory inventory, IItemManager itemManager, IDungeonHandler dungeonHandler, IUnitFinder unitFinder, IBattleLogManager battleLogManager);
}

public readonly struct ItemEffectContext
{
    /// <summary>
    /// アイテム使用者
    /// </summary>
    public ICollector Owner { get; }

    /// <summary>
    /// アイテム
    /// </summary>
    public ItemSetup ItemSetup { get; }

    /// <summary>
    /// アイテムManager
    /// </summary>
    public IItemManager ItemManager { get; }

    /// <summary>
    /// ダンジョン
    /// </summary>
    public IDungeonHandler DungeonHandler { get; }

    /// <summary>
    /// ユニット取得
    /// </summary>
    public IUnitFinder UnitFinder { get; }

    /// <summary>
    /// バトルログ
    /// </summary>
    public IBattleLogManager BattleLogManager { get; }

    public ItemEffectContext(ICollector owner, ItemSetup item, IItemManager itemManager, IDungeonHandler dungeonHandler, IUnitFinder unitFinder, IBattleLogManager battleLogManager)
    {
        Owner = owner;
        ItemSetup = item;
        ItemManager = itemManager;
        DungeonHandler = dungeonHandler;
        UnitFinder = unitFinder;
        BattleLogManager = battleLogManager;
    }
}

public class ItemEffectBase : ScriptableObject, IItemEffect
{
    private static readonly int THROW_DISTANCE = 10;

    /// <summary>
    /// アイテムを自分に使う
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="item"></param>
    /// <param name="inventory"></param>
    /// <param name="itemManager"></param>
    /// <param name="dungeonHandler"></param>
    /// <param name="unitFinder"></param>
    /// <param name="battleLogManager"></param>
    /// <returns></returns>
    async Task IItemEffect.Eat(ICollector owner, ItemSetup item, ITeamInventory inventory, IItemManager itemManager, IDungeonHandler dungeonHandler, IUnitFinder unitFinder, IBattleLogManager battleLogManager)
    {
        var ctx = new ItemEffectContext(owner, item, itemManager, dungeonHandler, unitFinder, battleLogManager);
        // Log
        if (ctx.Owner.RequireInterface<ICharaStatus>(out var status) == true)
            ctx.BattleLogManager.Log(status.CurrentStatus.OriginParam.GivenName + "は" + ctx.ItemSetup.ItemName + "を食べた");

        await EffectInternal(ctx);
        await PostEffect(owner, item, inventory);
    }

    /// <summary>
    /// アイテムを投げる
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="item"></param>
    /// <param name="inventory"></param>
    /// <param name="itemManager"></param>
    /// <param name="dungeonHandler"></param>
    /// <param name="unitFinder"></param>
    /// <param name="battleLogManager"></param>
    /// <returns></returns>
    async Task IItemEffect.ThrowStraight(ICollector owner, ItemSetup item, ITeamInventory inventory, IItemManager itemManager, IDungeonHandler dungeonHandler, IUnitFinder unitFinder, IBattleLogManager battleLogManager)
    {
        var ctx = new ItemEffectContext(owner, item, itemManager, dungeonHandler, unitFinder, battleLogManager);
        var target = await ThrowResult(ctx, THROW_DISTANCE);
        if (target != null)
            await EffectInternal(new ItemEffectContext(target, item, itemManager, dungeonHandler, unitFinder, battleLogManager));

        await PostEffect(owner, item, inventory);
    }

    /// <summary>
    /// アイテム投げて当たった敵を返す
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="distance"></param>
    /// <returns></returns>
    private async Task<ICollector> ThrowResult(ItemEffectContext ctx, int distance)
    {
        // Log
        var status = ctx.Owner.GetInterface<ICharaStatus>();
        ctx.BattleLogManager.Log(status.CurrentStatus.OriginParam.GivenName + "は" + ctx.ItemSetup.ItemName + "を投げた！");

        // 飛ばす方向
        var move = ctx.Owner.GetInterface<ICharaMove>();
        var dir = move.Direction;
        var dirV3 = dir.ToV3Int();
        var currentPos = move.Position;

        // ターゲットタイプ
        var targetType = ctx.Owner.GetInterface<ICharaTypeHolder>().TargetType;
        // ヒットしたキャラ
        ICollector target = null;
        bool isHit = false;

        int flyDistance;

        for (flyDistance = 1; flyDistance <= distance; flyDistance++)
        {
            // 攻撃マス
            var targetPos = currentPos + dirV3 * flyDistance;

            // 攻撃対象ユニットが存在するか調べる
            if (ctx.UnitFinder.TryGetSpecifiedPositionUnit(targetPos, out target, targetType) == true)
            {
                isHit = true;
                break;
            }

            // 地形チェック
            var terrain = ctx.DungeonHandler.GetCellId(targetPos);
            // 壁だったら走査終了
            if (terrain == TERRAIN_ID.WALL)
            {
                flyDistance--; // 手前に落ちる
                break;
            }
        }

        await ctx.ItemManager.FlyItem(ctx.ItemSetup, currentPos, dirV3 * flyDistance, !isHit);
        return target;
    }

    /// <summary>
    /// アイテム固有効果
    /// </summary>
    /// <param name="owner"></param>
    protected virtual Task EffectInternal(ItemEffectContext ctx)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// アイテム共通処理
    /// </summary>
    /// <param name="owner"></param>
    private async Task PostEffect(ICollector owner, ItemSetup item, ITeamInventory inventory)
    {
        // アイテム消費
        inventory.Consume(item);

        // アクション登録
        owner.GetInterface<ICharaLastActionHolder>().RegisterAction(CHARA_ACTION.ITEM_USE);

        // ターン消費
        await owner.GetInterface<ICharaTurn>().TurnEnd();
    }
}