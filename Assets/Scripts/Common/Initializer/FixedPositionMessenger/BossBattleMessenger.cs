using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public struct BossBattleInitializerInfo
{
    public Vector3 LeaderStartPos { get; }
    public Vector3 FriendStartPos { get; }
    public Vector3 LeaderEndPos { get; }
    public Vector3 FriendEndPos { get; }

    public Vector3 BossPos { get; }

    public BossBattleInitializerInfo(Vector3 ls, Vector3 fs, Vector3 le, Vector3 fe, Vector3 bp)
    {
        LeaderStartPos = ls;
        FriendStartPos = fs;
        LeaderEndPos = le;
        FriendEndPos = fe;

        BossPos = bp;
    }
}

public class BossBattleMessenger : MonoBehaviour
{
    [SerializeField]
    private Transform m_LeaderStartPos;
    [SerializeField]
    private Transform m_FriendStartPos;
    [SerializeField]
    private Transform m_LeaderEndPos;
    [SerializeField]
    private Transform m_FriendEndPos;

    [SerializeField]
    private Transform m_BossPos;

    private void Awake()
    {
        var info = new BossBattleInitializerInfo(m_LeaderStartPos.position, m_FriendStartPos.position, m_LeaderEndPos.position, m_FriendEndPos.position,
            m_BossPos.position);

        MessageBroker.Default.Publish(info);
    }
}
