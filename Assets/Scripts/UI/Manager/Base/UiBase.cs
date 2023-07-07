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
    void Initialize(CompositeDisposable disposable, OptionElement element, bool changeColor = true);

    /// <summary>
    /// 初期化
    /// </summary>
    /// <param name="disposables"></param>
    void Initialize(OptionElement element, bool changeColor = true);

    /// <summary>
    /// Ui表示・非表示
    /// </summary>
    /// <param name="active"></param>
    void SetActive(bool active);

    /// <summary>
    /// 選択肢変更
    /// </summary>
    /// <param name="option"></param>
    int AddOptionId(int option);

    /// <summary>
    /// 選択肢のメソッド実行
    /// </summary>
    void InvokeOptionMethod();

    /// <summary>
    /// 選択肢テキスト変更
    /// </summary>
    /// <param name="texts"></param>
    void ChangeText(string[] texts);

    /// <summary>
    /// テキストカラー更新
    /// </summary>
    void ChangeTextColor();

    /// <summary>
    /// テキストカラーをリセット
    /// </summary>
    void ResetTextColor();

    /// <summary>
    /// テキスト数
    /// </summary>
    int TextCount { get; }
}

[Serializable]
public class UiBase : IUiBase
{
    /// <summary>
    /// 操作するUi
    /// </summary>
    [SerializeField]
    protected GameObject m_Ui;

    /// <summary>
    /// 操作するテキストUi
    /// </summary>
    [SerializeField]
    protected Text[] m_Texts;
    int IUiBase.TextCount => m_Texts.Length;

    /// <summary>
    /// Ui表示中かどうか
    /// </summary>
    void IUiBase.SetActive(bool isActive) => m_Ui.SetActive(isActive);

    /// <summary>
    /// 選択肢Id
    /// </summary>
    private ReactiveProperty<int> m_OptionId = new ReactiveProperty<int>();
    private IObservable<int> OptionIdChanged => m_OptionId;
    int IUiBase.AddOptionId(int add)
    {
        int option = Mathf.Clamp(m_OptionId.Value + add, 0, m_OptionCount - 1);
        m_OptionId.Value = option;
        return m_OptionId.Value;
    }

    /// <summary>
    /// 選択肢のメソッド
    /// </summary>
    protected Subject<int> m_OptionMethod = new Subject<int>();
    void IUiBase.InvokeOptionMethod() => m_OptionMethod.OnNext(m_OptionId.Value);

    /// <summary>
    /// 選択肢の数
    /// </summary>
    protected int m_OptionCount;

    /// <summary>
    /// 初期化処理。手動で呼ぶ。
    /// </summary>
    void IUiBase.Initialize(CompositeDisposable disposable, OptionElement element, bool changeColor)
    {
        Initialize(element, changeColor);

        // 有効な選択肢の変更
        OptionIdChanged.SubscribeWithState(this, (option, self) => self.ChangeTextColor(option)).AddTo(disposable);
    }
    protected void Initialize(OptionElement element, bool changeColor)
    {
        // 選択肢メソッド初期化
        m_OptionMethod = element.OptionMethod;
        m_OptionCount = element.MethodCount;

        // 選択肢テキスト変更
        ChangeText(element.OptionTexts);

        // 有効中の選択肢初期化
        m_OptionId.Value = 0;

        if (changeColor == false)
            ResetTextColor();
    }
    void IUiBase.Initialize(OptionElement element, bool changeColor) => Initialize(element, changeColor);

    /// <summary>
    /// 選択肢テキスト変更
    /// </summary>
    /// <param name="texts"></param>
    private void ChangeText(string[] texts)
    {
        int i = 0;
        // 選択肢テキスト初期化
        for (i = 0; i < texts.Length; i++)
            m_Texts[i].text = texts[i];
        while (i < m_Texts.Length)
        {
            m_Texts[i].text = "";
            i++;
        }
    }
    void IUiBase.ChangeText(string[] texts) => ChangeText(texts);

    /// <summary>
    /// テキスト更新操作
    /// </summary>
    private void ChangeTextColor(int optionId)
    {
        optionId = Mathf.Clamp(optionId, 0, m_Texts.Length - 1);

        // 全部白にする
        ResetTextColor();

        //選択中の文字色更新
        m_Texts[optionId].color = Color.yellow;
    }
    void IUiBase.ChangeTextColor() => ChangeTextColor(m_OptionId.Value);

    private void ResetTextColor()
    {
        //選択肢の文字色更新
        for (int i = 0; i <= m_Texts.Length - 1; i++)
            m_Texts[i].color = Color.white;
    }
    void IUiBase.ResetTextColor() => ResetTextColor();
}
