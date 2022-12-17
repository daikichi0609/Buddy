using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using UnityEngine.UI;

/// <summary>
/// 左上メニューと説明のUi
/// </summary>

public class MenuManager : Singleton<MenuManager>
{
    /// <summary>
    /// マネージャークラス本体
    /// </summary>
    private MenuManager.MenuUi m_Manager = new MenuUi();
    public MenuUi GetManager
    {
        get => m_Manager;
    }

    protected override void Awake()
    {
        GameManager.Instance.GetUpdate
            .Subscribe(_ => GetManager.DetectInput());

        GetManager.IsActiveChanged.Subscribe(_ => GetManager.SwitchUi());

        GetManager.GetOptionId.Subscribe(_ => GetManager.UpdateText());
    }

    /// <summary>
    /// バッグを開く
    /// </summary>
    private void OpenBag()
    {
        BagManager.Instance.GetManager.IsActive = true;
    }

    /// <summary>
    /// ステータスの確認
    /// </summary>
    private void CheckStatus()
    {

    }

    public class MenuUi : UiBase
    {
        /// <summary>
        /// 選択肢の数
        /// </summary>
        protected override int OptionCount => 2;

        /// <summary>
        /// 選択肢のメソッド
        /// </summary>
        protected override List<System.Action> OptionMethods => new List<System.Action>
        {
            () => MenuManager.Instance.OpenBag(),
            () => MenuManager.Instance.CheckStatus(),
        };

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