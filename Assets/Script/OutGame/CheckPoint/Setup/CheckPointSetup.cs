using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(menuName = "MyScriptable/OutGame/CheckPoint")]
public class CheckPointSetup : ScriptableObject
{
    /// <summary>
    /// チェックポイント名
    /// </summary>
    [SerializeField, Header("チェックポイント名")]
    [ResizableTextArea]
    private string m_CheckPointName;
    public string CheckPointName => m_CheckPointName;

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
    /// 出発時会話フロー
    /// </summary>
    [SerializeField, Header("出発時会話フロー")]
    private GameObject m_DepartureFlow;
    public GameObject DepartureFlow => m_DepartureFlow;
}
