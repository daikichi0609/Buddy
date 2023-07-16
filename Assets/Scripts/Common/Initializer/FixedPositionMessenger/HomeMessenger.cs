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

public readonly struct HomeCharacterInfo
{
    public GameObject[] Prefabs { get; }
    public Transform[] Transforms { get; }
    public GameObject[] Flows { get; }

    public HomeCharacterInfo(GameObject[] prefabs, Transform[] transforms, GameObject[] flowcharts)
    {
        Prefabs = prefabs;
        Transforms = transforms;
        Flows = flowcharts;
    }
}

public class HomeMessenger : MonoBehaviour
{
    [SerializeField]
    private Transform m_LeaderPos;
    [SerializeField]
    private Transform m_FriendPos;

    [SerializeField]
    private GameObject[] m_Characters;
    [SerializeField]
    private Transform[] m_Positions;
    [SerializeField]
    private GameObject[] m_Flows;

    private void Awake()
    {
        var info = new HomeInitializerInfo(m_LeaderPos.position, m_FriendPos.position);
        MessageBroker.Default.Publish(info);

        var charaInfo = new HomeCharacterInfo(m_Characters, m_Positions, m_Flows);
        MessageBroker.Default.Publish(charaInfo);
    }
}
