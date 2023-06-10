using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public interface IItemEffect
{
    Task Effect(ICollector owner, IItemHandler item, ITeamInventory inventory, IItemManager itemManager, IDungeonHandler dungeonHandler, IUnitFinder unitFinder, IBattleLogManager battleLogManager, IDisposable disposable);
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
    /// <summary>
    /// アイテム効果
    /// </summary>
    /// <param name="owner"></param>
    public async Task Effect(ICollector owner, IItemHandler item, ITeamInventory inventory, IItemManager itemManager, IDungeonHandler dungeonHandler, IUnitFinder unitFinder, IBattleLogManager battleLogManager, IDisposable disposable)
    {
        await EffectInternal(new ItemEffectContext(owner, item.Setup, itemManager, dungeonHandler, unitFinder, battleLogManager));
        PostEffect(owner, item, inventory, disposable);
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
    private void PostEffect(ICollector owner, IItemHandler item, ITeamInventory inventory, IDisposable disposable)
    {
        // アイテム消費
        inventory.Consume(item);

        // アクション登録
        if (owner.RequireInterface<ICharaLastActionHolder>(out var last) == true)
            last.RegisterAction(CHARA_ACTION.ITEM_USE);

        // ターン消費
        if (owner.RequireInterface<ICharaTurn>(out var turn) == true)
            turn.TurnEnd();

        // Ui非有効化
        disposable.Dispose();
    }
}