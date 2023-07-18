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
    /// バディ会話フロー
    /// </summary>
    [SerializeField, Header("バディ会話フロー")]
    private GameObject[] m_FriendFlow;
    public GameObject GetFriendFlow(int index) => m_FriendFlow[index];
}
