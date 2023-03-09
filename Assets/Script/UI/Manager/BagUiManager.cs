using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using UnityEngine.UI;

/// <summary>
/// バッグUi
/// </summary>
public class BagUiManager : UiManagerBase<BagUiManager, IUiManager>, IUiManager
{
    private BagUiManager.BagUi m_UiInterface = new BagUi();
    protected override IUiBase UiInterface => m_UiInterface;

    protected override OptionElement CreateOptionElement()
    {
        var player = UnitHolder.Interface.PlayerList[0];
        var inventory = player.GetInterface<ICharaInventory>();
        var items = inventory.Items;
        int itemCount = items.Length;

        var effects = new Action[itemCount];
        var names = new string[itemCount];

        for (int i = 0; i < itemCount; i++)
        {
            var item = items[i];
            var name = item.Name;
            var effect = ItemEffect.GetEffect(name);
            effects[i] = () => effect.Effect(player, item);
            names[i] = name.ToString();
        }

        return new OptionElement(effects, names);
    }

    public class BagUi : UiBase
    {
        /// <summary>
        /// 操作するUi
        /// </summary>
        protected override GameObject Ui => UiHolder.Instance.BagUi;

        /// <summary>
        /// 操作するテキストUi
        /// </summary>
        protected override List<Text> Texts => UiHolder.Instance.ItemTexts;
    }
}