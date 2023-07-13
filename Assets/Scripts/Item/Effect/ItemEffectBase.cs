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
    Task Eat(ICollector owner, ItemSetup item, ITeamInventory inventory, IItemManager itemManager, IDungeonHandler dungeonHandler, IUnitFinder unitFinder, IBattleLogManager battleLogManager, IEffectHolder effectHolder, ISoundHolder sound);

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
    Task ThrowStraight(ICollector owner, ItemSetup item, ITeamInventory inventory, IItemManager itemManager, IDungeonHandler dungeonHandler, IUnitFinder unitFinder, IBattleLogManager battleLogManager, IEffectHolder effectHolder, ISoundHolder soundHolder);
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
    private static readonly string ITEM_THROW = "ItemThrow";
    private static readonly string ITEM_EAT = "ItemEat";

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
    private async Task Eat(ICollector owner, ItemSetup item, ITeamInventory inventory, IItemManager itemManager, IDungeonHandler dungeonHandler, IUnitFinder unitFinder,
        IBattleLogManager battleLogManager, IEffectHolder effectHolder, ISoundHolder soundHolder)
    {
        var ctx = new ItemEffectContext(owner, item, itemManager, dungeonHandler, unitFinder, battleLogManager);

        // Log
        if (ctx.Owner.RequireInterface<ICharaStatus>(out var status) == true)
            ctx.BattleLogManager.Log(status.CurrentStatus.OriginParam.GivenName + "は" + ctx.ItemSetup.ItemName + "を食べた");

        // 音
        if (soundHolder.TryGetSound(ITEM_EAT, out var sound) == true)
            sound.Play();

        // エフェクト
        if (effectHolder.TryGetEffect(ITEM_EAT, out var effect) == true)
        {
            var e = effect.Play(owner);
            await Task.Delay(500);
            e.Dispose();
        }

        // 空腹値回復
        if (ctx.Owner.RequireInterface<ICharaStarvation>(out var starvation) == true)
            starvation.RecoverHungry(item.Recover);

        await EffectInternal(ctx);
        await PostEffect(owner, item, inventory);
    }
    Task IItemEffect.Eat(ICollector owner, ItemSetup item, ITeamInventory inventory, IItemManager itemManager, IDungeonHandler dungeonHandler, IUnitFinder unitFinder,
        IBattleLogManager battleLogManager, IEffectHolder effectHolder, ISoundHolder sound) => Eat(owner, item, inventory, itemManager, dungeonHandler, unitFinder, battleLogManager, effectHolder, sound);

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
    async Task IItemEffect.ThrowStraight(ICollector owner, ItemSetup item, ITeamInventory inventory, IItemManager itemManager, IDungeonHandler dungeonHandler, IUnitFinder unitFinder,
        IBattleLogManager battleLogManager, IEffectHolder effectHolder, ISoundHolder soundHolder)
    {
        var ctx = new ItemEffectContext(owner, item, itemManager, dungeonHandler, unitFinder, battleLogManager);
        if (soundHolder.TryGetSound(ITEM_THROW, out var sound) == true)
            sound.Play();

        var target = await ThrowResult(ctx, THROW_DISTANCE);
        if (target != null)
        {
            if (item.CanEat == true)
                await Eat(target, item, inventory, itemManager, dungeonHandler, unitFinder, battleLogManager, effectHolder, soundHolder);
            else
                await EffectInternal(new ItemEffectContext(target, item, itemManager, dungeonHandler, unitFinder, battleLogManager));
        }

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

        var move = ctx.Owner.GetInterface<ICharaMove>();
        var pos = move.Position;
        var dirV3 = move.Direction.ToV3Int();
        var targetType = ctx.Owner.GetInterface<ICharaTypeHolder>().TargetType; // ターゲットタイプ

        var isHit = Positional.TryGetForwardUnit(pos, dirV3, THROW_DISTANCE, targetType, ctx.DungeonHandler, ctx.UnitFinder, out var target, out var flyDistance);
        await ctx.ItemManager.FlyItem(ctx.ItemSetup, pos, dirV3 * flyDistance, !isHit);
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

        // ターン消費
        await owner.GetInterface<ICharaTurn>().TurnEnd();
    }
}