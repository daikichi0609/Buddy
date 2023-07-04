using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using UnityEngine.UI;
using Zenject;
using NaughtyAttributes;

public interface ICharaSkillUiManager : IUiManager
{
}

/// <summary>
/// スキルUi
/// </summary>
public class CharaSkillUiManager : UiManagerBase, ICharaSkillUiManager
{
    [Serializable]
    private class CharacterSkillUi : UiBase { }

    [Inject]
    private IUnitHolder m_UnitHolder;

    [SerializeField]
    private Text m_DescriptionText;

    [SerializeField, ReorderableList]
    private CharacterSkillUi[] m_SkillUi;
    protected override IUiBase CurrentUiInterface => m_SkillUi[m_Depth.Value];

    private Subject<int>[] m_OptionMethods = new Subject<int>[] { new Subject<int>(), new Subject<int>() };
    protected override Subject<int> CurrentOptionSubject => m_OptionMethods[m_Depth.Value];

    protected override string FixLogText => "スキルを確認する。";

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
                IUiBase ui = self.m_SkillUi[depth.OldValue];
                ui.ResetTextColor();
            }
            else
            {
                IUiBase ui = self.m_SkillUi[depth.NewValue];
                ui.ChangeTextColor();
            }

            if (depth.NewValue != 1)
                m_DescriptionText.text = string.Empty;
        }).AddTo(this);

        OnOptionIdChange.SubscribeWithState(this, (id, self) =>
        {
            if (self.m_Depth.Value == 0)
            {
                self.m_Unit = self.m_UnitHolder.FriendList[id];
                var skillNames = self.CreateSkillNames(self.m_Unit, out var _);

                IUiBase ui = self.m_SkillUi[1];
                ui.ChangeText(skillNames);
            }

            if (self.m_Depth.Value == 1)
            {
                var skillHandler = self.m_Unit.GetInterface<ICharaSkillHandler>();
                if (skillHandler.TryGetSkill(id, out var skill) == true)
                    self.m_DescriptionText.text = skill.Description;
            }
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

        var skillNames = CreateSkillNames(m_UnitHolder.FriendList[0], out var skillCount);
        var e1 = new OptionElement(m_OptionMethods[1], skillNames, skillCount);

        return new OptionElement[] { e0, e1 };
    }

    /// <summary>
    /// スキル名配列
    /// </summary>
    /// <param name="unit"></param>
    /// <returns></returns>
    private string[] CreateSkillNames(ICollector unit, out int skillCount)
    {
        string[] skillNames = new string[4];
        skillCount = 0;
        var skillHandler = unit.GetInterface<ICharaSkillHandler>();
        for (int i = 0; i < skillNames.Length; i++)
        {
            if (skillHandler.TryGetSkill(i, out var skill) == true)
            {
                skillNames[i] = skill.Name;
                skillCount++;
            }
            else
                skillNames[i] = "------";
        }
        return skillNames;
    }

    /// <summary>
    /// Uiの初期化
    /// </summary>
    protected override void InitializeUi()
    {
        // 初期化
        var option = CreateOptionElement();

        for (int i = 0; i < m_SkillUi.Length; i++)
        {
            IUiBase ui = m_SkillUi[i];
            bool changeColor = i == 0 ? true : false;
            ui.Initialize(m_Disposables, option[i], changeColor);
            ui.SetActive(true); // 表示
        }

        m_Unit = m_UnitHolder.FriendList[0];
    }

    /// <summary>
    /// Uiの終了時
    /// </summary>
    protected override void FinalizeUi()
    {
        // Ui非表示
        for (int i = 0; i < m_SkillUi.Length; i++)
        {
            IUiBase ui = m_SkillUi[i];
            ui.SetActive(false); // 非表示
        }
    }
}

