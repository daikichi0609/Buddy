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
    /// 操作するUi
    /// </summary>
    [SerializeField]
    protected GameObject m_Ui;

    /// <summary>
    /// 操作するテキストUi
    /// </summary>
    [SerializeField]
    protected List<Text> m_Texts;

    /// <summary>
    /// Ui表示中かどうか
    /// </summary>
    void IUiBase.SetActive(bool isActive) => m_Ui.SetActive(isActive);

    /// <summary>
    /// 選択肢Id
    /// </summary>
    private ReactiveProperty<int> m_OptionId = new ReactiveProperty<int>();
    private IObservable<int> OptionIdChanged => m_OptionId;
    void IUiBase.AddOptionId(int add)
    {
        int option = Mathf.Clamp(m_OptionId.Value + add, 0, m_OptionCount - 1);
        m_OptionId.Value = option;
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
    protected void Initialize(CompositeDisposable disposable, OptionElement element)
    {
        // 有効な選択肢の変更
        OptionIdChanged.SubscribeWithState(this, (option, self) => self.OnChangeActiveOption(ref option)).AddTo(disposable);

        // 選択肢メソッド初期化
        m_OptionMethod = element.OptionMethod;
        m_OptionCount = element.MethodCount;

        int i = 0;
        // 選択肢テキスト初期化
        for (i = 0; i < element.OptionTexts.Length; i++)
            m_Texts[i].text = element.OptionTexts[i];
        while (i < m_Texts.Count)
        {
            m_Texts[i].text = "";
            i++;
        }

        // 有効中の選択肢初期化
        m_OptionId.Value = 0;
    }

    void IUiBase.Initialize(CompositeDisposable disposable, OptionElement element) => Initialize(disposable, element);

    /// <summary>
    /// テキスト更新操作
    /// </summary>
    private void OnChangeActiveOption(ref int optionId)
    {
        optionId = Mathf.Clamp(optionId, 0, m_Texts.Count - 1);

        //選択肢の文字色更新
        for (int i = 0; i <= m_Texts.Count - 1; i++)
            m_Texts[i].color = Color.white;

        //選択中の文字色更新
        m_Texts[optionId].color = Color.yellow;
    }
}
