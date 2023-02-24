using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using UnityEngine.UI;

/// <summary>
/// 選択肢Ui作成時の情報
/// </summary>
public readonly struct OptionElement
{
    /// <summary>
    /// 選択肢メソッド
    /// </summary>
    public Action[] OptionMethods { get; }

    /// <summary>
    /// 選択肢テキスト
    /// </summary>
    public string[] OptionTexts { get; }

    public OptionElement(Action[] methods, string[] texts)
    {
        OptionMethods = methods;
        OptionTexts = texts;
    }
}

public interface IUiManager : ISingleton
{
    void Activate();
}

/// <summary>
/// 左上メニューと説明のUi
/// </summary>
public abstract class UiManagerBase<T, IT> : Singleton<T, IT>, IUiManager where T : MonoBehaviour, IUiManager where IT : class, IUiManager
{
    /// <summary>
    /// Ui操作インターフェイス
    /// </summary>
    protected abstract IUiBase UiInterface { get; }

    /// <summary>
    /// 選択肢のメソッド
    /// </summary>
    protected abstract OptionElement CreateOptionElement();

    /// <summary>
    /// Ui閉じるときに呼ぶ
    /// </summary>
    protected Action CloseUiEvent { get; set; }

    /// <summary>
    /// Uiがアクティブかどうか
    /// </summary>
    protected bool IsActive => CloseUiEvent != null;

    /// <summary>
    /// 購読解除用
    /// </summary>
    private CompositeDisposable m_Disposables = new CompositeDisposable();

    protected override void Awake()
    {
        base.Awake();

        // Input検知購読
        InputManager.Interface.InputEvent.Subscribe(input =>
        {
            DetectInput(input.KeyCodeFlag);
        }).AddTo(this);
    }

    private void DetectInput(KeyCodeFlag flag)
    {
        if (IsActive == false)
            return;

        if (TurnManager.Interface.NoOneActing == false)
            return;

        // Qで閉じる
        if (flag.HasBitFlag(KeyCodeFlag.Q))
        {
            Deactivate();
            return;
        }

        //決定ボタン 該当メソッド実行
        if (flag.HasBitFlag(KeyCodeFlag.Return))
        {
            UiInterface.InvokeOptionMethod();
            return;
        }

        //上にカーソル移動
        if (flag.HasBitFlag(KeyCodeFlag.W))
        {
            UiInterface.AddOptionId(-1);
            return;
        }

        //下にカーソル移動
        if (flag.HasBitFlag(KeyCodeFlag.S))
        {
            UiInterface.AddOptionId(1);
            return;
        }
    }

    /// <summary>
    /// Ui有効化
    /// </summary>
    protected virtual void Activate()
    {
        CloseUiEvent = InputManager.Interface.SetActiveUi(this.UiInterface);
        UiInterface.Initialize(m_Disposables, CreateOptionElement());
        UiInterface.SetActive(IsActive);
    }
    void IUiManager.Activate() => Activate();

    /// <summary>
    /// Ui無効化
    /// </summary>
    protected virtual void Deactivate()
    {
        UiInterface.SetActive(false);
        m_Disposables.Clear();

        CloseUiEvent?.Invoke();
        CloseUiEvent = null;
    }
}