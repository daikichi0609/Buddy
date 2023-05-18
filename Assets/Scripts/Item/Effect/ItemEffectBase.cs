using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IItemEffect
{
    void Effect(ICollector owner, IItemHandler item);
}

public class ItemEffectBase : ScriptableObject, IItemEffect
{
    /// <summary>
    /// アイテム効果
    /// </summary>
    /// <param name="owner"></param>
    public void Effect(ICollector owner, IItemHandler item)
    {
        EffectInternal(owner, item);
        PostEffect(owner, item);
    }

    /// <summary>
    /// アイテム固有効果
    /// </summary>
    /// <param name="owner"></param>
    protected virtual void EffectInternal(ICollector owner, IItemHandler item)
    {

    }

    /// <summary>
    /// アイテム共通処理
    /// </summary>
    /// <param name="owner"></param>
    private void PostEffect(ICollector owner, IItemHandler item)
    {
        // Ui非有効化
        BagUiManager.Interface.Deactive(false);

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