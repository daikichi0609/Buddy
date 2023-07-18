using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UniRx;

public enum TIMELINE_TYPE
{
    RAGON_INTRO = 0,
    BERRY_INTRO = 1,
    DORCHE_INTRO = 2,
}

public enum TIMELINE_FINISH_TYPE
{
    PLAYABLE,
}

public readonly struct RegisterTimelineMessage
{
    public PlayableDirector Director { get; }
    public TIMELINE_TYPE Key { get; }

    public RegisterTimelineMessage(PlayableDirector director, TIMELINE_TYPE key)
    {
        Director = director;
        Key = key;
    }
}

public readonly struct FinishTimelineMessage
{
    public TIMELINE_TYPE Type { get; }
    public TIMELINE_FINISH_TYPE FinishType { get; }

    public FinishTimelineMessage(TIMELINE_TYPE type, TIMELINE_FINISH_TYPE finishType)
    {
        Type = type;
        FinishType = finishType;
    }
}

public class TimelineRegister : MonoBehaviour
{
    [SerializeField]
    private PlayableDirector m_Director;

    [SerializeField]
    private TIMELINE_TYPE m_Type;

    [SerializeField]
    private TIMELINE_FINISH_TYPE m_FinishType;

    private void Awake() => MessageBroker.Default.Publish(new RegisterTimelineMessage(m_Director, m_Type));

    public void OnFinish() => MessageBroker.Default.Publish(new FinishTimelineMessage(m_Type, m_FinishType));
}
