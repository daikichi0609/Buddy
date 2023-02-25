using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using UnityEngine.UI;

/// <summary>
/// Uiパラメタの操作
/// </summary>
public interface IUiBase
{
    /// <summary>
    /// 初期化
    /// </summary>
    /// <param name="disposables"></param>
    void Initialize(CompositeDisposable disposable, OptionElement element);

    /// <summary>
    /// Ui表示・非表示
    /// </summary>
    /// <param name="active"></param>
    void SetActive(bool active);

    /// <summary>
    /// 選択肢変更
    /// </summary>
    /// <param name="option"></param>
    void AddOptionId(int option);

    /// <summary>
    /// 選択肢のメソッド実行
    /// </summary>
    void InvokeOptionMethod();
}

public abstract class UiBase : IUiBase
{
    /// <summary>
    /// Ui表示中かどうか
    /// </summary>
    private ReactiveProperty<bool> m_IsActive = new ReactiveProperty<bool>(false);
    private IObservable<bool> IsActiveChanged => m_IsActive.Skip(1);
    void IUiBase.SetActive(bool active) => m_IsActive.Value = active;

    /// <summary>
    /// 選択肢Id
    /// </summary>
    private ReactiveProperty<int> m_OptionId = new ReactiveProperty<int>();
    private IObservable<int> OptionIdChanged => m_OptionId;
    void IUiBase.AddOptionId(int add)
    {
        int option = Mathf.Clamp(m_OptionId.Value + add, 0, OptionCount);
        m_OptionId.Value = option;
    }

    /// <summary>
    /// 選択肢の数
    /// </summary>
    protected int OptionCount => OptionMethods.Length;

    /// <summary>
    /// 選択肢のメソッド
    /// </summary>
    protected Action[] OptionMethods { get; set; }
    void IUiBase.InvokeOptionMethod() => OptionMethods[m_OptionId.Value]?.Invoke();

    /// <summary>
    /// 操作するUi
    /// </summary>
    protected virtual GameObject Ui { get; }

    /// <summary>
    /// 操作するテキストUi
    /// </summary>
    protected virtual List<Text> Texts { get; }

    /// <summary>
    /// 初期化処理。手動で呼ぶ。
    /// </summary>
    protected void Initialize(CompositeDisposable disposable, OptionElement element)
    {
        // Ui表示・非表示
        IsActiveChanged.Subscribe(active => OnChangeUiActive(active)).AddTo(disposable);

        // 有効な選択肢の変更
        OptionIdChanged.Subscribe(option => OnChangeActiveOption(ref option)).AddTo(disposable);

        // 選択肢メソッド初期化
        OptionMethods = element.OptionMethods;

        // 選択肢テキスト初期化
        for (int i = 0; i < element.OptionMethods?.Length; i++)
            Texts[i].text = element.OptionTexts[i];

        // 有効中の選択肢初期化
        m_OptionId.Value = 0;
    }

    void IUiBase.Initialize(CompositeDisposable disposable, OptionElement element) => Initialize(disposable, element);

    /// <summary>
    /// Ui表示・非表示操作
    /// </summary>
    private void OnChangeUiActive(bool isActive) => Ui.SetActive(isActive);

    /// <summary>
    /// テキスト更新操作
    /// </summary>
    private void OnChangeActiveOption(ref int optionId)
    {
        optionId = Mathf.Clamp(optionId, 0, Texts.Count - 1);

        //選択肢の文字色更新
        for (int i = 0; i <= Texts.Count - 1; i++)
            Texts[i].color = Color.white;

        //選択中の文字色更新
        Texts[optionId].color = Color.yellow;
    }
}
