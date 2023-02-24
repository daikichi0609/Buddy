using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System;

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

public class YesorNoQuestionUiManager : UiManagerBase<YesorNoQuestionUiManager, IYesorNoQuestionUiManager>, IYesorNoQuestionUiManager
{
    private YesorNoQuestionUiManager.QuestionUi m_Interface = new QuestionUi();
    protected override IUiBase UiInterface => m_Interface;

    protected override OptionElement CreateOptionElement()
    {
        var e = m_Question switch
        {
            QUESTION_TYPE.STAIRS => new OptionElement
            (new Action[2] { () => GameManager.Instance.UpToNextFloor(), () => Deactivate() }, OptionText),

            QUESTION_TYPE.NONE => new OptionElement(),
            _ => new OptionElement()
        } ;

        return e;
    }

    /// <summary>
    /// 質問タイプ
    /// Activateする前にセットする
    /// </summary>
    private QUESTION_TYPE m_Question = QUESTION_TYPE.NONE;
    void IYesorNoQuestionUiManager.SetQuestion(QUESTION_TYPE q) => m_Question = q;

    private static readonly string[] OptionText = new string[2] { "はい", "いいえ" };

    /// <summary>
    /// 質問文セット
    /// </summary>
    protected override void Activate()
    {
        base.Activate();

        var s = m_Question switch
        {
            QUESTION_TYPE.STAIRS => "先に進みますか？",

            QUESTION_TYPE.NONE => "",
            _ => ""
        };

        UiHolder.Instance.QuestionText.text = s;
    }

    protected override void Deactivate()
    {
        base.Deactivate();
    }

    public class QuestionUi : UiBase
    {
        /// <summary>
        /// 操作するUi
        /// </summary>
        protected override GameObject Ui => UiHolder.Instance.QuestionAndChoiceUi;

        /// <summary>
        /// 操作するテキストUi
        /// </summary>
        protected override List<Text> Texts => UiHolder.Instance.OptionTextList;
    }
}