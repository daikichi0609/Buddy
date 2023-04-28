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
        var player = UnitHolder.Interface.FriendList[0];
        var inventory = player.GetInterface<ICharaInventory>();
        var items = inventory.Items;
        int itemCount = items.Length;

        var effects = new Action[itemCount];

        int itemTextCount = m_UiInterface.ItemElementCount;
        var names = new string[itemTextCount];

        int index = 0;
        for (index = 0; index < itemCount; index++)
        {
            var item = items[index];
            var name = item.Setup.ItemName;
            var effect = item.Setup.Effect;
            effects[index] = () => effect.Effect(player, item);
            names[index] = name.ToString();
        }
        while (index < itemTextCount)
        {
            names[index] = "----------";
            index++;
        }

        return new OptionElement(effects, names);
    }

    protected override void Deactivate()
    {
        base.Deactivate();
        MenuUiManager.Interface.Activate();
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

        /// <summary>
        /// アイテムの要素数
        /// </summary>
        public int ItemElementCount => Texts.Count;
    }
}