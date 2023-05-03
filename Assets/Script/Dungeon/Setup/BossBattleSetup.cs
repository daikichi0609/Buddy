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
    /// ステージ
    /// </summary>
    [SerializeField, Header("ステージ")]
    private GameObject m_Stage;
    public GameObject Stage => m_Stage;

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
    private GameObject m_DefeatFlow;
    public GameObject DefeatFlow => m_DefeatFlow;
}
