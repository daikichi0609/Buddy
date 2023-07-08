using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System;
using Zenject;

/// <summary>
/// 質問タイプ
/// </summary>
public enum QUESTION_TYPE
{
    NONE,
    STAIRS,
}

public interface IYesorNoQuestionUiManager : IUiManager
{
    void SetQuestion(QUESTION_TYPE q);
}

/// <summary>
/// 選択肢と質問文のUi
/// </summary>

public class YesorNoQuestionUiManager : UiManagerBase, IYesorNoQuestionUiManager
{
    [Inject]
    protected IDungeonProgressManager m_DungeonProgressManager;

    protected override bool IsActiveMiniMap => true;
    protected override int MaxDepth => 1;
    protected override string FixLogText => "";

    /// <summary>
    /// 質問テキスト
    /// </summary>
    [SerializeField]
    private Text m_QuestionText;

    /// <summary>
    /// 質問タイプ
    /// Activateする前にセットする
    /// </summary>
    private QUESTION_TYPE m_Question = QUESTION_TYPE.NONE;
    void IYesorNoQuestionUiManager.SetQuestion(QUESTION_TYPE q) => m_Question = q;

    private static readonly string[] OptionText = new string[2] { "はい", "いいえ" };

    protected override OptionElement[] CreateOptionElement()
    {
        var s = m_Question switch
        {
            QUESTION_TYPE.STAIRS => "先に進みますか？",

            QUESTION_TYPE.NONE or _ => "",
        };
        m_QuestionText.text = s;

        if (m_Question == QUESTION_TYPE.STAIRS)
        {
            var disposable = m_OptionMethods[0].SubscribeWithState(this, (index, self) =>
            {
                if (index == 0)
                    self.m_DungeonProgressManager.NextFloor();
                else if (index == 1)
                    self.Deactivate();
            }).AddTo(this);

            m_Disposables.Add(disposable);
        }

        return new OptionElement[] { new OptionElement(m_OptionMethods[0], OptionText) };
    }
}

/// <summary>
/// Ui非表示メッセージ
/// </summary>
public readonly struct CloseUiMessage
{
    public QUESTION_TYPE Type { get; }

    public CloseUiMessage(QUESTION_TYPE type)
    {
        Type = type;
    }
}