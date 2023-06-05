using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IItemEffect
{
    void Effect(ICollector owner, IItemHandler item, IDungeonHandler dungeonHandler, IUnitFinder unitFinder, IBagUiManager bagUiManager, IBattleLogManager battleLogManager);
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

    public ItemEffectContext(ICollector owner, ItemSetup item, IDungeonHandler dungeonHandler, IUnitFinder unitFinder, IBattleLogManager battleLogManager)
    {
        Owner = owner;
        ItemSetup = item;
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
    public void Effect(ICollector owner, IItemHandler item, IDungeonHandler dungeonHandler, IUnitFinder unitFinder, IBagUiManager bagUiManager, IBattleLogManager battleLogManager)
    {
        EffectInternal(new ItemEffectContext(owner, item.Setup, dungeonHandler, unitFinder, battleLogManager));
        PostEffect(owner, item, bagUiManager);
    }

    /// <summary>
    /// アイテム固有効果
    /// </summary>
    /// <param name="owner"></param>
    protected virtual void EffectInternal(ItemEffectContext ctx)
    {

    }

    /// <summary>
    /// アイテム共通処理
    /// </summary>
    /// <param name="owner"></param>
    private void PostEffect(ICollector owner, IItemHandler item, IBagUiManager bagUiManager)
    {
        // Ui非有効化
        bagUiManager.Deactive(false);

        // アイテム消費
        if (owner.RequireInterface<ICharaInventory>(out var inventory) == true)
            inventory.Consume(item);

        // アクション登録
        if (owner.RequireInterface<ICharaLastActionHolder>(out var last) == true)
            last.RegisterAction(CHARA_ACTION.ITEM_USE);

        // ターン消費
        if (owner.RequireInterface<ICharaTurn>(out var turn) == true)
            turn.TurnEnd();
    }
}