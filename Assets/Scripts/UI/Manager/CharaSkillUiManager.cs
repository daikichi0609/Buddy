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
    [Inject]
    private ICharaUiManager m_CharaUiManager;

    [SerializeField]
    private Text m_DescriptionText;

    [SerializeField]
    private GameObject[] m_CheckMarks;

    protected override bool IsActiveMiniMap => false;
    protected override int MaxDepth => 2;
    protected override string FixLogText => "スキルを確認する。";
    private int m_UnitIndex;
    private ICollector CurrentUnit => m_UnitHolder.FriendList[m_UnitIndex];

    protected override void Awake()
    {
        base.Awake();

        // Depth上げ
        m_OptionMethods[0].SubscribeWithState(this, (option, self) => self.m_Depth.Value++).AddTo(this);

        m_OptionMethods[1].SubscribeWithState(this, (option, self) =>
        {
            var skillHandler = self.CurrentUnit.GetInterface<ICharaSkillHandler>();
            bool result = skillHandler.SwitchActivate(option);
            m_CheckMarks[option].SetActive(result);
        }).AddTo(this);

        // Depth変更時
        m_Depth
            .Zip(m_Depth.Skip(1), (x, y) => new { OldValue = x, NewValue = y })
            .SubscribeWithState(this, (depth, self) =>
        {
            // 戻ったなら前Uiのテキストカラーリセット
            if (depth.NewValue < depth.OldValue)
            {
                IUiBase ui = self.m_UiManaged[depth.OldValue];
                ui.ResetTextColor();
            }
            // 進んだならそのUiのテキストカラー有効化
            else
            {
                IUiBase ui = self.m_UiManaged[depth.NewValue];
                ui.ChangeTextColor();
            }

            // 説明文なし
            if (depth.NewValue != 1)
                self.m_DescriptionText.text = string.Empty;
            // 説明文更新
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
        m_Disposables.Add(Disposable.CreateWithState(this, self => self.m_UnitIndex = 0));

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
        IUiBase ui = m_UiManaged[1];
        for (int i = 0; i < ui.TextCount; i++)
        {
            var skillHandler = CurrentUnit.GetInterface<ICharaSkillHandler>();
            if (skillHandler.TryGetSkill(i, out var skill) == true)
                m_CheckMarks[i].SetActive(skill.IsActive);
            else
                m_CheckMarks[i].SetActive(false);
        }

        var skillNames = CreateSkillNames(CurrentUnit, out var skillCount);
        return new OptionElement(m_OptionMethods[1], skillNames, skillCount);
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
    /// 説明文更新
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="index"></param>
    private void ChangeDescriptionText(ICollector unit, int index)
    {
        var skillHandler = unit.GetInterface<ICharaSkillHandler>();
        if (skillHandler.TryGetSkill(index, out var skill) == true)
            m_DescriptionText.text = skill.Description;
    }
}

