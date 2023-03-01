using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IItemEffect
{
    void Effect(ICollector owner, IItem item);
}

public abstract class ItemEffect : IItemEffect
{
    /// <summary>
    /// アイテム名
    /// </summary>
    protected abstract ITEM_NAME ItemName { get; }

    /// <summary>
    /// アイテム効果
    /// </summary>
    /// <param name="owner"></param>
    public void Effect(ICollector owner, IItem item)
    {
        EffectInternal(owner);
        PostEffect(owner, item);
    }

    /// <summary>
    /// アイテム固有効果
    /// </summary>
    /// <param name="owner"></param>
    protected abstract void EffectInternal(ICollector owner);

    /// <summary>
    /// アイテム共通処理
    /// </summary>
    /// <param name="owner"></param>
    private void PostEffect(ICollector owner, IItem item)
    {
        // Ui非有効化
        BagUiManager.Interface.Deactive();

        // アイテム消費
        if (owner.RequireInterface<ICharaInventory>(out var inventory) == true)
            inventory.Consume(item);

        // ターン消費
        if (owner.RequireInterface<ICharaTurn>(out var turn) == true)
            turn.TurnEnd();
    }

    /// <summary>
    /// アイテム名からアイテム効果取得
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static ItemEffect GetEffect(ITEM_NAME name)
    {
        switch (name)
        {
            case ITEM_NAME.APPLE:
                return new Apple();
        }

        return null;
    }
}