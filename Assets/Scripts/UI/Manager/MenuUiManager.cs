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
    [Serializable]
    public class MenuUi : UiBase { }

    [Inject]
    private IBagUiManager m_BagUiManager;
    [Inject]
    private ICharaSkillUiManager m_SkillUiManager;

    [SerializeField]
    private MenuUiManager.MenuUi m_UiInterface = new MenuUi();
    protected override IUiBase CurrentUiInterface => m_UiInterface;

    private Subject<int> m_OptionMethod = new Subject<int>();
    protected override Subject<int> CurrentOptionSubject => m_OptionMethod;

    protected override string FixLogText => "コマンドを選択する。";

    protected override OptionElement[] CreateOptionElement()
    {
        return new OptionElement[] { new OptionElement(m_OptionMethod, new string[5] { "バッグ", "スキル", "かしこさ", "作戦", "閉じる" }) };
    }

    protected override void Awake()
    {
        base.Awake();

        SubscribeMenuOpen();

        m_OptionMethod.SubscribeWithState(this, (index, self) =>
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
        m_SkillUiManager.Activate(this);
    }

    /// <summary>
    /// スキル確認
    /// </summary>
    private void CheckStrategy()
    {
        Deactivate();
    }
}