using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(menuName = "MyScriptable/OutGame/Home")]
public class HomeSetup : ScriptableObject
{
    /// <summary>
    /// ホーム進行度
    /// </summary>
    [SerializeField, Header("ホーム進行度")]
    private int m_Progress;
    public int Progress => m_Progress;

    /// <summary>
    /// ステージ
    /// </summary>
    [SerializeField, Header("ステージ")]
    private GameObject m_Stage;
    public GameObject Stage => m_Stage;

    /// <summary>
    /// バディ会話フロー
    /// </summary>
    [SerializeField, Header("バディ会話フロー")]
    private GameObject m_FriendFlow;
    public GameObject FriendFlow => m_FriendFlow;
}
