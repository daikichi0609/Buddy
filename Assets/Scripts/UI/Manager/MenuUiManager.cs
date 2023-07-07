using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using UnityEngine.UI;
using Zenject;

public interface IMenuUiManager : IUiManager
{
}

/// <summary>
/// 左上メニューと説明のUi
/// </summary>
public class MenuUiManager : UiManagerBase, IMenuUiManager
{
    [Inject]
    private IBagUiManager m_BagUiManager;
    [Inject]
    private ICharaSkillUiManager m_SkillUiManager;
    [Inject]
    private ICharaClevernessUiManager m_ClevernessUiManager;

    protected override int MaxDepth => 1;
    protected override string FixLogText => "コマンドを選択する。";

    protected override void Awake()
    {
        base.Awake();

        SubscribeMenuOpen();

        m_OptionMethods[0].SubscribeWithState(this, (index, self) =>
        {
            // バッグを開く
            if (index == 0)
                self.OpenBag();
            // スキル確認
            else if (index == 1)
                self.CheckSkill();
            // 賢さ確認
            else if (index == 2)
                self.CheckCleverness();
            // 作戦
            else if (index == 3)
                self.CheckStrategy();
            // 閉じる
            else
                self.Deactivate();
        }).AddTo(this);
    }

    protected override OptionElement[] CreateOptionElement()
    {
        return new OptionElement[] { new OptionElement(m_OptionMethods[0], new string[5] { "バッグ", "スキル", "かしこさ", "作戦", "閉じる" }) };
    }

    /// <summary>
    /// メニューを開く
    /// </summary>
    private void SubscribeMenuOpen()
    {
        m_InputManager.InputStartEvent.SubscribeWithState(this, (input, self) =>
        {
            if (self.m_InputManager.IsUiPopUp == false && self.m_TurnManager.NoOneActing == true && input.KeyCodeFlag.HasBitFlag(KeyCodeFlag.M))
            {
                self.Activate();
                if (m_SoundHolder.TryGetSound(DECIDE, out var sound) == true)
                    sound.Play();
            }
        }).AddTo(this);
    }

    /// <summary>
    /// バッグを開く
    /// </summary>
    private void OpenBag()
    {
        Deactivate();
        m_BagUiManager.Activate(this);
    }

    /// <summary>
    /// スキル確認
    /// </summary>
    private void CheckSkill()
    {
        Deactivate();
        m_SkillUiManager.Activate(this);
    }

    /// <summary>
    /// 賢さ確認
    /// </summary>
    private void CheckCleverness()
    {
        Deactivate();
        m_ClevernessUiManager.Activate(this);
    }

    /// <summary>
    /// スキル確認
    /// </summary>
    private void CheckStrategy()
    {
        Deactivate();
    }
}