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
    [Serializable]
    public class QuestionUi : UiBase
    {
        [SerializeField]
        public Text m_QuestionText;
    }

    [Inject]
    protected IDungeonProgressManager m_DungeonProgressManager;

    [SerializeField]
    private YesorNoQuestionUiManager.QuestionUi m_Interface = new QuestionUi();
    protected override IUiBase CurrentUiInterface => m_Interface;

    private Subject<int> m_OptionMethod = new Subject<int>();
    protected override Subject<int> CurrentOptionSubject => m_OptionMethod;

    protected override string FixLogText => "";

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
        m_Interface.m_QuestionText.text = s;

        if (m_Question == QUESTION_TYPE.STAIRS)
        {
            var disposable = m_OptionMethod.SubscribeWithState(this, (index, self) =>
            {
                if (index == 0)
                    self.m_DungeonProgressManager.NextFloor();
                else if (index == 1)
                    self.Deactivate();
            }).AddTo(this);

            m_Disposables.Add(disposable);
        }

        return new OptionElement[] { new OptionElement(m_OptionMethod, OptionText) };
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