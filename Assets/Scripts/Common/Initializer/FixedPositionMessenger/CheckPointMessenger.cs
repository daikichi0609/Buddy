using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public struct CheckPointInitializerInfo
{
    public Vector3 LeaderStartPos { get; }
    public Vector3 FriendStartPos { get; }
    public Vector3 LeaderEndPos { get; }
    public Vector3 FriendEndPos { get; }

    public Vector3 LeaderPos { get; }
    public Vector3 FriendPos { get; }

    public CheckPointInitializerInfo(Vector3 ls, Vector3 fs, Vector3 le, Vector3 fe, Vector3 lp, Vector3 fp)
    {
        LeaderStartPos = ls;
        FriendStartPos = fs;
        LeaderEndPos = le;
        FriendEndPos = fe;

        LeaderPos = lp;
        FriendPos = fp;
    }
}

public class CheckPointMessenger : MonoBehaviour
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
    private Transform m_LeaderPos;
    [SerializeField]
    private Transform m_FriendPos;

    private void Awake()
    {
        var info = new CheckPointInitializerInfo(m_LeaderStartPos.position, m_FriendStartPos.position, m_LeaderEndPos.position, m_FriendEndPos.position,
            m_LeaderPos.position, m_FriendPos.position);

        MessageBroker.Default.Publish(info);
    }
}
