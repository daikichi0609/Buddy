using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System;

/// <summary>
/// 選択肢と質問文のUi
/// </summary>

public class QuestionManager :SingletonMonoBehaviour<QuestionManager>
{
    /// <summary>
    /// マネージャークラス本体
    /// </summary>
    private QuestionManager.QuestionUi m_Manager = new QuestionUi();
    public QuestionUi GetManager
    {
        get => m_Manager;
    }

    /// <summary>
    /// Logテキスト情報
    /// </summary>
    private ReactiveProperty<QuestionInfo> m_Log = new ReactiveProperty<QuestionInfo>();

    public IObservable<QuestionInfo> LogChanged
    {
        get { return m_Log.Skip(1); }
    }

    public QuestionInfo Log
    {
        private get => m_Log.Value;
        set => m_Log.Value = value;
    }

    protected override void Awake()
    {
        GameManager.Instance.GetUpdate
            .Subscribe(_ => GetManager.DetectInput());

        GetManager.IsActiveChanged.Subscribe(_ => GetManager.SwitchUi());

        GetManager.GetOptionId.Subscribe(_ => GetManager.UpdateText());
    }

    public class QuestionUi : UiBase
    {
        /// <summary>
        /// 選択肢の数
        /// </summary>
        protected override int OptionCount => QuestionManager.Instance.Log.OptionNum;

        /// <summary>
        /// 選択肢のメソッド
        /// </summary>
        protected override List<Action> OptionMethods => QuestionManager.Instance.Log.OptionMethod;

        /// <summary>
        /// 操作するUi
        /// </summary>
        protected override GameObject Ui => UiHolder.Instance.QuestionAndChoiceUi;

        /// <summary>
        /// 操作するテキストUi
        /// </summary>
        protected override List<Text> Texts => UiHolder.Instance.OptionTextList;

        /// <summary>
        /// Subscribeする 一回読み込み
        /// </summary>
        public override void UpdateText()
        {
            base.UpdateText();

            //質問文の更新
            UiHolder.Instance.QuestionText.text = QuestionManager.Instance.Log.Question;
        }
    }
}