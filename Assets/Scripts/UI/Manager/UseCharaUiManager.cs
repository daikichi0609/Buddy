using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using Zenject;
using System;

public interface IUseCharaUiManager : IUiManagerImp
{
    ItemSetup ItemSetup { get; set; }
}

public class UseCharaUiManager : UiManagerBase, IUseCharaUiManager
{
    [Inject]
    private IUnitHolder m_UnitHolder;
    [Inject]
    private IUnitFinder m_UnitFinder;
    [Inject]
    private ITeamInventory m_TeamInventory;
    [Inject]
    private IItemManager m_ItemManager;
    [Inject]
    private IDungeonHandler m_DungeonHandler;
    [Inject]
    private IEffectHolder m_EffectHolder;

    protected override bool IsActiveMiniMap => false;
    protected override string FixLogText => "誰が？";
    protected override int MaxDepth => 1;

    /// <summary>
    /// 使うアイテムセットアップ
    /// </summary>
    private ItemSetup m_ItemSetup;
    ItemSetup IUseCharaUiManager.ItemSetup { get => m_ItemSetup; set => m_ItemSetup = value; }

    protected override OptionElement[] CreateOptionElement()
    {
        List<string> strings = new List<string>();

        var use = m_OptionMethods[0].SubscribeWithState(this, (index, self) =>
        {
            // アクション登録
            var player = self.m_UnitHolder.Player;
            player.GetInterface<ICharaLastActionHolder>().RegisterAction(CHARA_ACTION.ITEM_USE);

            self.DeactivateAll();
            self.m_ItemSetup.Effect.Eat(self.m_UnitHolder.FriendList[index], self.m_ItemSetup, self.m_TeamInventory, self.m_ItemManager,
                self.m_DungeonHandler, self.m_UnitFinder, self.m_BattleLogManager, self.m_EffectHolder, self.m_SoundHolder);
        });
        m_Disposables.Add(use);

        foreach (var unit in m_UnitHolder.FriendList)
        {
            var status = unit.GetInterface<ICharaStatus>();
            strings.Add(status.CurrentStatus.OriginParam.GivenName);
        }

        return new OptionElement[] { new OptionElement(m_OptionMethods[0], strings.ToArray(), m_UnitHolder.FriendList.Count) };
    }
}