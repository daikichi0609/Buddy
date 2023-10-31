using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(menuName = "MyScriptable/OutGame/Home")]
public class HomeSetup : ScriptableObject
{
    /// <summary>
    /// ステージ
    /// </summary>
    [SerializeField, Header("ステージ")]
    private GameObject m_Stage;
    public GameObject Stage => m_Stage;

    /// <summary>
    /// BGM
    /// </summary>
    [SerializeField, Header("BGM")]
    private GameObject m_BGM;
    public GameObject BGM => m_BGM;

    /// <summary>
    /// 攻略中バディ会話フロー
    /// </summary>
    [SerializeField, Header("バディ攻略中会話フロー")]
    private GameObject[] m_FriendFlow;
    public bool TryGetFriendFlow(int index, out GameObject friend)
    {
        friend = null;
        if (index < 0 || index >= m_FriendFlow.Length)
            return false;

        friend = m_FriendFlow[index];
        return friend != null;
    }

    /// <summary>
    /// 攻略済みバディ会話フロー
    /// </summary>
    [SerializeField, Header("バディ攻略後会話フロー")]
    private GameObject[] m_FriendCompletedFlow;
    public bool TryGetFriendCompletedFlow(int index, out GameObject friendFlow)
    {
        friendFlow = null;
        if (index < 0 || index >= m_FriendFlow.Length)
            return false;

        friendFlow = m_FriendCompletedFlow[index];
        return friendFlow != null;
    }

    /// <summary>
    /// 敗北時フロー
    /// </summary>
    [SerializeField, Header("敗北時フロー")]
    private GameObject m_LoseBackFlow;
    public GameObject LoseBackFlow => m_LoseBackFlow;
}
