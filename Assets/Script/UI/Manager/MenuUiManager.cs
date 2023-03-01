using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using UnityEngine.UI;

/// <summary>
/// 左上メニューと説明のUi
/// </summary>
public class MenuUiManager : UiManagerBase<MenuUiManager, IUiManager>, IUiManager
{
    private MenuUiManager.MenuUi m_UiInterface = new MenuUi();
    protected override IUiBase UiInterface => m_UiInterface;

    protected override OptionElement CreateOptionElement()
    {
        return new OptionElement
        (new Action[2] { () => OpenBag(), () => CheckStatus() },
        new string[2] { "バッグ", "ステータス" });
    }

    /// <summary>
    /// メニュー開く購読
    /// </summary>
    private IDisposable m_OpenMenuDisposable;

    protected override void Awake()
    {
        base.Awake();

        SubscribeMenuOpen();
    }

    /// <summary>
    /// メニューを開く
    /// </summary>
    private void SubscribeMenuOpen()
    {
        m_OpenMenuDisposable = InputManager.Interface.InputStartEvent.Subscribe(input =>
        {
            if (IsActive == false && TurnManager.Interface.NoOneActing == true && input.KeyCodeFlag.HasBitFlag(KeyCodeFlag.Q))
                Activate();
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
    protected override void Deactivate()
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
        BagUiManager.Interface.Activate();
    }

    /// <summary>
    /// ステータスの確認
    /// </summary>
    private void CheckStatus()
    {
        Deactivate();
    }

    public class MenuUi : UiBase
    {
        /// <summary>
        /// 操作するUi
        /// </summary>
        protected override GameObject Ui => UiHolder.Instance.MenuUi;

        /// <summary>
        /// 操作するテキストUi
        /// </summary>
        protected override List<Text> Texts => UiHolder.Instance.MenuText;
    }
}