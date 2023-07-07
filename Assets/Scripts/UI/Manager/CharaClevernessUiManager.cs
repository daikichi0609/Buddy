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
    [Serializable]
    private class CharacterClevernessUi : UiBase { }

    [Inject]
    private IUnitHolder m_UnitHolder;
    [Inject]
    private ICharaUiManager m_CharaUiManager;

    [SerializeField]
    private Text m_DescriptionText;

    [SerializeField, ReorderableList]
    private CharacterClevernessUi[] m_ClevernesslUi;
    protected override IUiBase CurrentUiInterface => m_ClevernesslUi[m_Depth.Value];

    private Subject<int>[] m_OptionMethods = new Subject<int>[] { new Subject<int>(), new Subject<int>() };
    protected override Subject<int> CurrentOptionSubject => m_OptionMethods[m_Depth.Value];

    protected override string FixLogText => "かしこさを確認する。";

    private ICollector m_Unit;

    protected override void Awake()
    {
        base.Awake();

        m_OptionMethods[0].SubscribeWithState(this, (option, self) =>
        {
            self.m_Depth.Value++;
        }).AddTo(this);

        m_Depth
            .Zip(m_Depth.Skip(1), (x, y) => new { OldValue = x, NewValue = y })
            .SubscribeWithState(this, (depth, self) =>
            {

                if (depth.NewValue < depth.OldValue)
                {
                    IUiBase ui = self.m_ClevernesslUi[depth.OldValue];
                    ui.ResetTextColor();
                }
                else
                {
                    IUiBase ui = self.m_ClevernesslUi[depth.NewValue];
                    ui.ChangeTextColor();
                }

                if (depth.NewValue != 1)
                    self.m_DescriptionText.text = string.Empty;
                else
                    self.ChangeDescriptionText(self.m_Unit, 0);

            }).AddTo(this);

        OnOptionIdChange.SubscribeWithState(this, (id, self) =>
        {
            if (self.m_Depth.Value == 0)
            {
                self.m_Unit = self.m_UnitHolder.FriendList[id];
                var clevernessNames = self.CreateClevernessNames(self.m_Unit, out var _);

                IUiBase ui = self.m_ClevernesslUi[1];
                ui.ChangeText(clevernessNames);
            }

            if (self.m_Depth.Value == 1)
                self.ChangeDescriptionText(self.m_Unit, id);
        }).AddTo(this);
    }

    protected override OptionElement[] CreateOptionElement()
    {
        string[] unitNames = new string[2];
        for (int i = 0; i < unitNames.Length; i++)
        {
            var unit = m_UnitHolder.FriendList[i];
            var status = unit.GetInterface<ICharaStatus>();
            unitNames[i] = status.CurrentStatus.OriginParam.GivenName;
        }

        var e0 = new OptionElement(m_OptionMethods[0], unitNames);

        var skillNames = CreateClevernessNames(m_UnitHolder.FriendList[0], out var skillCount);
        var e1 = new OptionElement(m_OptionMethods[1], skillNames, skillCount);

        return new OptionElement[] { e0, e1 };
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

    /// <summary>
    /// Uiの初期化
    /// </summary>
    protected override void InitializeUi()
    {
        // 初期化
        var option = CreateOptionElement();

        for (int i = 0; i < m_ClevernesslUi.Length; i++)
        {
            IUiBase ui = m_ClevernesslUi[i];
            bool changeColor = i == 0 ? true : false;
            ui.Initialize(m_Disposables, option[i], changeColor);
            ui.SetActive(true); // 表示
        }

        m_Unit = m_UnitHolder.FriendList[0];

        var disposable = m_CharaUiManager.SetActive(false);
        m_Disposables.Add(disposable);
    }

    /// <summary>
    /// Uiの終了時
    /// </summary>
    protected override void FinalizeUi()
    {
        // Ui非表示
        for (int i = 0; i < m_ClevernesslUi.Length; i++)
        {
            IUiBase ui = m_ClevernesslUi[i];
            ui.SetActive(false); // 非表示
        }
    }
}

