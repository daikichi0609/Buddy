using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using UnityEngine.UI;
using Zenject;

public interface IBagUiManager : IUiManager
{
}

/// <summary>
/// バッグUi
/// </summary>
public class BagUiManager : UiManagerBase, IBagUiManager
{
    [Serializable]
    public class BagUi : UiBase
    {
        /// <summary>
        /// アイテムの要素数
        /// </summary>
        public int ItemElementCount => m_Texts.Count;
    }

    [Inject]
    private IMenuUiManager m_MenuUiManager;
    [Inject]
    private IItemManager m_ItemManager;
    [Inject]
    private IDungeonHandler m_DungeonHandler;
    [Inject]
    private IUnitHolder m_UnitHolder;
    [Inject]
    private IUnitFinder m_UnitFinder;

    [SerializeField]
    private BagUiManager.BagUi m_UiInterface = new BagUi();
    protected override IUiBase UiInterface => m_UiInterface;

    protected override OptionElement CreateOptionElement()
    {
        var player = m_UnitHolder.FriendList[0];
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
            effects[index] = async () =>
            {
                var disposable = m_TurnManager.RequestProhibitAction(null);
                Deactivate(false);
                await effect.Effect(player, item, m_ItemManager, m_DungeonHandler, m_UnitFinder, m_BattleLogManager, disposable);
            };
            names[index] = name.ToString();
        }
        while (index < itemTextCount)
        {
            names[index] = "----------";
            index++;
        }

        return new OptionElement(effects, names);
    }

    protected override void Deactivate(bool back)
    {
        base.Deactivate();
        if (back == true)
            m_MenuUiManager.Activate();
    }
}