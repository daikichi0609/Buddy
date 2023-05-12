using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(menuName = "MyScriptable/Dungeon/BossBattle")]
public class BossBattleSetup : ScriptableObject
{
    /// <summary>
    /// ステージ名
    /// </summary>
    [SerializeField, Header("ステージ名")]
    [ResizableTextArea]
    private string m_BossBattleName;
    public string BossBattleName => m_BossBattleName;

    /// <summary>
    /// ステージ名 2行目
    /// </summary>
    [SerializeField, Header("ステージ名 2行目")]
    [ResizableTextArea]
    private string m_WhereName;
    public string WhereName => m_WhereName;

    /*
    /// <summary>
    /// ステージ
    /// </summary>
    [SerializeField, Header("ステージ")]
    private GameObject m_Stage;
    public GameObject Stage => m_Stage;
    */

    /// <summary>
    /// ボスキャラクター
    /// </summary>
    [SerializeField, Header("ボスキャラクター")]
    [Expandable]
    private CharacterSetup m_BossCharacterSetup;
    public CharacterSetup BossCharacterSetup => m_BossCharacterSetup;

    /// <summary>
    /// 到着時会話フロー
    /// </summary>
    [SerializeField, Header("到着時会話フロー")]
    private GameObject m_ArrivalFlow;
    public GameObject ArrivalFlow => m_ArrivalFlow;

    /// <summary>
    /// 撃破時会話フロー
    /// </summary>
    [SerializeField, Header("撃破時会話フロー")]
    private GameObject m_DefeatedFlow;
    public GameObject DefeatedFlow => m_DefeatedFlow;

    /// <summary>
    /// BGM
    /// </summary>
    [SerializeField, Header("BGM")]
    private GameObject m_BGM;
    public GameObject BGM => m_BGM;
}
