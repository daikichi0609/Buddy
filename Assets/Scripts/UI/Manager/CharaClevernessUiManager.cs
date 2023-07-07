using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using UnityEngine.UI;
using Zenject;
using NaughtyAttributes;

public interface ICharaClevernessUiManager : IUiManager
{
}

/// <summary>
/// スキルUi
/// </summary>
public class CharaClevernessUiManager : UiManagerBase, ICharaClevernessUiManager
{
    [Inject]
    private IUnitHolder m_UnitHolder;
    [Inject]
    private ICharaUiManager m_CharaUiManager;

    [SerializeField]
    private Text m_DescriptionText;

    protected override int MaxDepth => 2;
    protected override string FixLogText => "かしこさを確認する。";
    private int m_UnitIndex;
    private ICollector CurrentUnit => m_UnitHolder.FriendList[m_UnitIndex];

    protected override void Awake()
    {
        base.Awake();

        m_OptionMethods[0].SubscribeWithState(this, (option, self) => self.m_Depth.Value++).AddTo(this);

        m_Depth
            .Zip(m_Depth.Skip(1), (x, y) => new { OldValue = x, NewValue = y })
            .SubscribeWithState(this, (depth, self) =>
            {
                if (depth.NewValue < depth.OldValue)
                {
                    IUiBase ui = self.m_UiManaged[depth.OldValue];
                    ui.ResetTextColor();
                }
                else
                {
                    IUiBase ui = self.m_UiManaged[depth.NewValue];
                    ui.ChangeTextColor();
                }

                if (depth.NewValue != 1)
                    self.m_DescriptionText.text = string.Empty;
                else
                    self.ChangeDescriptionText(CurrentUnit, 0);

            }).AddTo(this);

        OnOptionIdChange.SubscribeWithState(this, (id, self) =>
        {
            if (self.m_Depth.Value == 0)
            {
                m_UnitIndex = id;
                IUiBase ui = self.m_UiManaged[1];
                var e1 = CreateOptionElement1();
                ui.Initialize(e1, false);
            }

            if (self.m_Depth.Value == 1)
                self.ChangeDescriptionText(CurrentUnit, id);
        }).AddTo(this);
    }

    protected override OptionElement[] CreateOptionElement()
    {
        var disposable = m_CharaUiManager.SetActive(false);
        m_Disposables.Add(disposable);

        var e0 = CreateOptionElement0();
        var e1 = CreateOptionElement1();

        return new OptionElement[] { e0, e1 };
    }

    private OptionElement CreateOptionElement0()
    {
        string[] unitNames = new string[2];
        for (int i = 0; i < unitNames.Length; i++)
        {
            var unit = m_UnitHolder.FriendList[i];
            var status = unit.GetInterface<ICharaStatus>();
            unitNames[i] = status.CurrentStatus.OriginParam.GivenName;
        }

        return new OptionElement(m_OptionMethods[0], unitNames);
    }

    private OptionElement CreateOptionElement1()
    {
        var skillNames = CreateClevernessNames(CurrentUnit, out var skillCount);
        return new OptionElement(m_OptionMethods[1], skillNames, skillCount);
    }

    /// <summary>
    /// スキル名配列
    /// </summary>
    /// <param name="unit"></param>
    /// <returns></returns>
    private string[] CreateClevernessNames(ICollector unit, out int clevernessCount)
    {
        string[] clevernessNames = new string[4];
        clevernessCount = 0;
        var clevernessHandler = unit.GetInterface<ICharaClevernessHandler>();
        for (int i = 0; i < clevernessNames.Length; i++)
        {
            if (clevernessHandler.TryGetCleverness(i, out var cleverness) == true)
            {
                clevernessNames[i] = cleverness.Name;
                clevernessCount++;
            }
            else
                clevernessNames[i] = "------";
        }
        return clevernessNames;
    }

    /// <summary>
    /// 説明文更新
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="index"></param>
    private void ChangeDescriptionText(ICollector unit, int index)
    {
        var skillHandler = unit.GetInterface<ICharaClevernessHandler>();
        if (skillHandler.TryGetCleverness(index, out var cleverness) == true)
            m_DescriptionText.text = cleverness.Description;
    }
}

