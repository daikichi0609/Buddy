using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using Zenject;
using System;

public interface IItemUseUiManager : IUiManagerImp
{
    ItemSetup ItemSetup { get; set; }
}

public class ItemUseUiManager : UiManagerBase, IItemUseUiManager
{
    [Inject]
    private IUnitFinder m_UnitFinder;
    [Inject]
    private ITeamInventory m_TeamInventory;
    [Inject]
    private IItemManager m_ItemManager;
    [Inject]
    private IDungeonHandler m_DungeonHandler;
    [Inject]
    private IUseCharaUiManager m_UseCharaUiManager;
    [Inject]
    private IEffectHolder m_EffectHolder;

    protected override bool IsActiveMiniMap => false;
    protected override int MaxDepth => 1;
    protected override string FixLogText => "どうする？";

    /// <summary>
    /// 使うアイテムセットアップ
    /// </summary>
    private ItemSetup m_ItemSetup;
    ItemSetup IItemUseUiManager.ItemSetup { get => m_ItemSetup; set => m_ItemSetup = value; }

    protected override OptionElement[] CreateOptionElement()
    {
        int optionIndex = 0;
        List<string> strings = new List<string>();

        // 食べる
        if (m_ItemSetup.CanEat == true)
        {
            var eat = m_OptionMethods[0].SubscribeWithState((optionIndex, this), (index, tuple) =>
            {
                if (tuple.optionIndex == index)
                {
                    var self = tuple.Item2;
                    self.m_UseCharaUiManager.ItemSetup = m_ItemSetup;
                    self.m_UseCharaUiManager.Activate(this);
                }
            });
            m_Disposables.Add(eat);
            optionIndex++;
            strings.Add("食べる");
        }

        // 投げる
        var throwStraight = m_OptionMethods[0].SubscribeWithState((optionIndex, this), (index, tuple) =>
        {
            if (tuple.optionIndex == index)
            {
                // アクション登録
                var player = tuple.Item2.m_UnitHolder.Player;
                player.GetInterface<ICharaLastActionHolder>().RegisterAction(CHARA_ACTION.ITEM_USE);

                var self = tuple.Item2;
                self.DeactivateAll();
                self.m_ItemSetup.Effect.ThrowStraight(self.m_UnitHolder.Player, self.m_ItemSetup, self.m_TeamInventory, self.m_ItemManager,
                    self.m_DungeonHandler, self.m_UnitFinder, self.m_BattleLogManager, self.m_EffectHolder, self.m_SoundHolder);
            }
        });
        m_Disposables.Add(throwStraight);
        optionIndex++;
        strings.Add("投げる");

        // やめる
        var quit = m_OptionMethods[0].SubscribeWithState((optionIndex, this), (index, tuple) =>
        {
            if (tuple.optionIndex == index)
            {
                IUiManagerImp self = tuple.Item2;
                self.Deactivate();
            }
        });
        m_Disposables.Add(quit);
        optionIndex++;
        strings.Add("やめる");

        return new OptionElement[] { new OptionElement(m_OptionMethods[0], strings.ToArray(), optionIndex) };
    }
}
