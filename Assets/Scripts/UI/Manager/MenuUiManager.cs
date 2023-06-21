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

    [SerializeField]
    private MenuUiManager.MenuUi m_UiInterface = new MenuUi();
    protected override IUiBase UiInterface => m_UiInterface;

    protected override OptionElement CreateOptionElement() => new OptionElement(m_OptionMethod, new string[2] { "バッグ", "ステータス" });

    /// <summary>
    /// メニュー開く購読
    /// </summary>
    private IDisposable m_OpenMenuDisposable;

    protected void Awake()
    {
        SubscribeMenuOpen();

        m_OptionMethod.SubscribeWithState(this, (index, self) =>
        {
            if (index == 0)
                self.OpenBag();
            else if (index == 1)
                self.CheckStatus();
        }).AddTo(this);
    }

    /// <summary>
    /// メニューを開く
    /// </summary>
    private void SubscribeMenuOpen()
    {
        m_OpenMenuDisposable = m_InputManager.InputStartEvent.SubscribeWithState(this, (input, self) =>
        {
            if (self.m_IsOperatable == false && self.m_TurnManager.NoOneActing == true && input.KeyCodeFlag.HasBitFlag(KeyCodeFlag.M))
                self.Activate();
        }).AddTo(this);
    }

    /// <summary>
    /// メニュー開く購読破棄
    /// </summary>
    protected override void Activate()
    {
        base.Activate();
        m_OpenMenuDisposable.Dispose();
    }

    /// <summary>
    /// メニュー開く再購読
    /// </summary>
    protected override void Deactivate(bool openParent = true)
    {
        base.Deactivate();
        SubscribeMenuOpen();
    }

    /// <summary>
    /// バッグを開く
    /// </summary>
    private void OpenBag()
    {
        Deactivate();

        // 新しくUiを開く
        m_BagUiManager.Activate(this);
    }

    /// <summary>
    /// ステータスの確認
    /// </summary>
    private void CheckStatus()
    {
        Deactivate();
    }
}