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
    [Inject]
    private IItemUseUiManager m_ItemUseUiManager;
    [Inject]
    private ITeamInventory m_TeamInventory;

    protected override bool IsActiveMiniMap => false;
    protected override string FixLogText => "アイテムを選択する。";
    protected override int MaxDepth => 1;

    protected override OptionElement[] CreateOptionElement()
    {
        var items = m_TeamInventory.Items; // 全てのアイテム
        int itemCount = items.Length; // アイテム数

        int itemTextCount = ((IUiBase)m_UiManaged[0]).TextCount; // バッグのアイテム数
        var names = new string[itemTextCount]; // バッグのアイテム名

        int index = 0;
        for (index = 0; index < itemCount; index++)
        {
            var item = items[index];
            var name = item.ItemName;
            var disposable = m_OptionMethods[0].SubscribeWithState((index, m_ItemUseUiManager, item), (i, tuple) =>
            {
                if (i == tuple.index)
                {
                    tuple.m_ItemUseUiManager.ItemSetup = tuple.item;
                    tuple.m_ItemUseUiManager.Activate(this);
                }
            });
            m_Disposables.Add(disposable);
            names[index] = name.ToString();
        }
        int methodCount = index;
        while (index < itemTextCount)
        {
            names[index] = "----------";
            index++;
        }

        return new OptionElement[] { new OptionElement(m_OptionMethods[0], names, methodCount) };
    }
}

