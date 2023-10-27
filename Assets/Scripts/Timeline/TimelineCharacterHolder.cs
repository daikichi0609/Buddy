using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public readonly struct TimelineCharacterMessage
{
    public TimelineCharacterHolder TimelineCharacterHolder { get; }

    public TimelineCharacterMessage(TimelineCharacterHolder holder) => TimelineCharacterHolder = holder;
}


public class TimelineCharacterHolder : MonoBehaviour
{
    [SerializeField]
    private GameObject m_Logue;
    public GameObject Logue => m_Logue;
    [SerializeField]
    private GameObject m_Ragon;
    public GameObject Ragon => m_Ragon;
    [SerializeField]
    private GameObject m_Berry;
    public GameObject Berry => m_Berry;
    [SerializeField]
    private GameObject m_Dorch;
    public GameObject Dorch => m_Dorch;

    private void Awake()
    {
        MessageBroker.Default.Publish(new TimelineCharacterMessage(this));
    }
}
