using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiHolder : Singleton<UiHolder>
{
    /// <summary>
    /// Ui全体
    /// </summary>

    //キャンバス
    [SerializeField] private GameObject m_Canvas;
    public GameObject Canvas => m_Canvas;

    /// <summary>
    /// キャラUi関連
    /// </summary>

    //キャラクターUIプレハブ
    [SerializeField] private GameObject m_CharacterUi;
    public GameObject CharacterUi
    {
        get { return m_CharacterUi; }
    }

    /// <summary>
    /// メニュー
    /// </summary>

    //メニューUi
    [SerializeField] private GameObject m_MenuUi;
    public GameObject MenuUi
    {
        get { return m_MenuUi; }
    }

    //メニューテキスト
    [SerializeField] private List<Text> m_MenuText;
    public List<Text> MenuText
    {
        get { return m_MenuText; }
    }

    /// <summary>
    /// バッグ
    /// </summary>

    //バッグUi
    [SerializeField] private GameObject m_BagUi;
    public GameObject BagUi
    {
        get { return m_BagUi; }
    }

    //アイテム一覧テキスト
    [SerializeField] private List<Text> m_ItemTexts;
    public List<Text> ItemTexts
    {
        get { return m_ItemTexts; }
    }

    /// <summary>
    /// Log
    /// </summary>

    //質問文と選択肢だけの汎用UI（シーンに存在）
    [SerializeField] private GameObject m_QuestionAndChoiceUi;
    public GameObject QuestionAndChoiceUi
    {
        get { return m_QuestionAndChoiceUi; }
    }

    //質問文
    [SerializeField] private Text m_QuestionText;
    public Text QuestionText
    {
        get { return m_QuestionText; }
    }

    //選択肢の文格納List
    [SerializeField] private List<Text> m_OptionTextList;
    public List<Text> OptionTextList
    {
        get { return m_OptionTextList; }
        set { m_OptionTextList = value; }
    }
}
