using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UniRx;
using NaughtyAttributes;
using System;

public enum TIMELINE_TYPE
{
    NONE = -1,
    INTRO = 0,
    RAGON_INTRO = 1,
    BERRY_INTRO = 2,
    DORCHE_INTRO = 3,
}

public readonly struct RegisterTimelineMessage
{
    public GameObject Camera { get; }
    public PlayableDirector Director { get; }
    public TIMELINE_TYPE Key { get; }

    public RegisterTimelineMessage(GameObject camera, PlayableDirector director, TIMELINE_TYPE key)
    {
        Camera = camera;
        Director = director;
        Key = key;
    }
}

public readonly struct FinishTimelineNextTimelineMessage
{
    /// <summary>
    /// 終了したタイムライン
    /// </summary>
    public TIMELINE_TYPE Type { get; }

    /// <summary>
    /// 次のタイムライン
    /// </summary>
    public TIMELINE_TYPE NextTimeline { get; }

    public FinishTimelineNextTimelineMessage(TIMELINE_TYPE type, TIMELINE_TYPE next)
    {
        Type = type;
        NextTimeline = next;
    }
}

public readonly struct FinishTimelineNextFungusMessage
{
    /// <summary>
    /// 終了したタイムライン
    /// </summary>
    public TIMELINE_TYPE Type { get; }

    /// <summary>
    /// 次のFungus
    /// </summary>
    public GameObject FungusObject { get; }

    /// <summary>
    /// リーダー座標
    /// </summary>
    public Vector3 PlayerPos { get; }

    /// <summary>
    /// フレンド座標
    /// </summary>
    public Vector3 FriendPos { get; }

    public FinishTimelineNextFungusMessage(TIMELINE_TYPE type, GameObject fungus, Vector3 playerPos, Vector3 friendPos)
    {
        Type = type;
        FungusObject = fungus;
        PlayerPos = playerPos;
        FriendPos = friendPos;
    }
}

public readonly struct FinishTimelineBePlayableMessage
{
    /// <summary>
    /// 終了したタイムライン
    /// </summary>
    public TIMELINE_TYPE Type { get; }

    public FinishTimelineBePlayableMessage(TIMELINE_TYPE type)
    {
        Type = type;
    }
}

public readonly struct CropMessage
{
    public double Duration { get; }
    public string Text { get; }

    public CropMessage(double duration, string text)
    {
        Duration = duration;
        Text = text;
    }
}

public readonly struct WhiteOutMessage
{
    public float Speed { get; }
    public float Time { get; }

    public WhiteOutMessage(float speed, float time)
    {
        Speed = speed;
        Time = time;
    }
}

public class TimelineRegister : MonoBehaviour
{
    private enum TIMELINE_FINISH_TYPE
    {
        PLAYER_PLAYABLE,
        NEXT_TIMELINE,
        FUNGUS_EVENT,
    }

    [BoxGroup("共通設定")]
    [SerializeField, Header("カメラ")]
    private GameObject m_Camera;

    [BoxGroup("共通設定")]
    [SerializeField, Header("タイムライン")]
    private PlayableDirector m_Director;

    [BoxGroup("共通設定")]
    [SerializeField, Header("タイムライン・タイプ")]
    private TIMELINE_TYPE m_Type;

    [BoxGroup("終了イベント")]
    [SerializeField, Header("再生終了後イベント")]
    private TIMELINE_FINISH_TYPE m_FinishType;

    [BoxGroup("終了イベント")]
    [SerializeField, Header("次に再生するタイムライン")]
    private TIMELINE_TYPE m_NextTimeline;

    [BoxGroup("終了イベント")]
    [SerializeField, Header("Fungus")]
    private GameObject m_Fungus;

    [BoxGroup("終了イベント")]
    [SerializeField, Header("リーダー座標")]
    private Transform m_LeaderPos;

    [BoxGroup("終了イベント")]
    [SerializeField, Header("フレンド座標")]
    private Transform m_FriendPos;

    private static readonly float FADE_SPEED = 0.5f;
    private static readonly float FADE_TIME = 1f;

    private void Awake()
    {
        m_Camera.SetActive(false);
        MessageBroker.Default.Publish(new RegisterTimelineMessage(m_Camera, m_Director, m_Type));
    }

    /// <summary>
    /// ホワイトアウト
    /// </summary>
    public void WhiteOut()
    {
        MessageBroker.Default.Publish(new WhiteOutMessage(FADE_SPEED, FADE_TIME));
    }

    /// <summary>
    /// 再生終了イベント
    /// </summary>
    public void OnFinish()
    {
        switch (m_FinishType)
        {
            case TIMELINE_FINISH_TYPE.PLAYER_PLAYABLE:
                MessageBroker.Default.Publish(new FinishTimelineBePlayableMessage(m_Type));
                break;

            case TIMELINE_FINISH_TYPE.NEXT_TIMELINE:
                MessageBroker.Default.Publish(new FinishTimelineNextTimelineMessage(m_Type, m_NextTimeline));
                break;

            case TIMELINE_FINISH_TYPE.FUNGUS_EVENT:
                MessageBroker.Default.Publish(new FinishTimelineNextFungusMessage(m_Type, m_Fungus, m_LeaderPos.position, m_FriendPos.position));
                break;
        }

    }
}
