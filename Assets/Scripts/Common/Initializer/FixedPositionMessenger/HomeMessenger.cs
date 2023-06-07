using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public struct HomeInitializerInfo
{
    public Vector3 LeaderPos { get; }
    public Vector3 FriendPos { get; }

    public HomeInitializerInfo(Vector3 lp, Vector3 fp)
    {
        LeaderPos = lp;
        FriendPos = fp;
    }
}

public class HomeMessenger : MonoBehaviour
{
    [SerializeField]
    private Transform m_LeaderPos;
    [SerializeField]
    private Transform m_FriendPos;

    private void Awake()
    {
        var info = new HomeInitializerInfo(m_LeaderPos.position, m_FriendPos.position);

        MessageBroker.Default.Publish(info);
    }
}
