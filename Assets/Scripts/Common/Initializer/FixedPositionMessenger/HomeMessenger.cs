using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public struct HomeInitializerInfo
{
    public Transform LeaderTransform { get; }
    public Transform FriendTransform { get; }

    public HomeInitializerInfo(Transform lt, Transform ft)
    {
        LeaderTransform = lt;
        FriendTransform = ft;
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
        var info = new HomeInitializerInfo(m_LeaderPos, m_FriendPos);
        MessageBroker.Default.Publish(info);

        var charaInfo = new HomeCharacterInfo(m_Characters, m_Positions, m_Flows);
        MessageBroker.Default.Publish(charaInfo);
    }
}
