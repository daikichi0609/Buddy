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

public enum TIMELINE_FINISH_TYPE
{
    PLAYER_PLAYABLE,
    NEXT_TIMELINE,
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

public readonly struct FinishTimelineMessage
{
    public TIMELINE_TYPE Type { get; }
    public TIMELINE_FINISH_TYPE FinishType { get; }
    public TIMELINE_TYPE NextTimeline { get; }

    public FinishTimelineMessage(TIMELINE_TYPE type, TIMELINE_FINISH_TYPE finishType, TIMELINE_TYPE next)
    {
        Type = type;
        FinishType = finishType;
        NextTimeline = next;
    }
}

public readonly struct DialogMessage
{
    public DialogSetup.DialogPack Dialog { get; }

    public DialogMessage(DialogSetup.DialogPack dialogPack)
    {
        Dialog = dialogPack;
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
    [SerializeField, Header("カメラ")]
    private GameObject m_Camera;

    [SerializeField, Header("タイムライン")]
    private PlayableDirector m_Director;

    [SerializeField, Header("タイムライン・タイプ")]
    private TIMELINE_TYPE m_Type;

    [SerializeField, Header("再生終了後イベント")]
    private TIMELINE_FINISH_TYPE m_FinishType;

    [SerializeField, Header("次に再生するタイムライン")]
    private TIMELINE_TYPE m_NextTimeline;

    [SerializeField, Header("ダイアログセットアップ")]
    [Expandable]
    private DialogSetup m_DialogSetup;
    private int m_CurrentIndex;

    private static readonly float FADE_SPEED = 0.5f;
    private static readonly float FADE_TIME = 1f;

    private void Awake()
    {
        m_Camera.SetActive(false);
        MessageBroker.Default.Publish(new RegisterTimelineMessage(m_Camera, m_Director, m_Type));
    }

    /// <summary>
    /// 再生終了イベント
    /// </summary>
    public void OnFinish() => MessageBroker.Default.Publish(new FinishTimelineMessage(m_Type, m_FinishType, m_NextTimeline));

    /// <summary>
    /// 字幕表示
    /// </summary>
    public void ShowDialog()
    {
        var packs = m_DialogSetup.DialogPacks;
        MessageBroker.Default.Publish(new DialogMessage(packs[m_CurrentIndex]));
        if (++m_CurrentIndex >= packs.Length)
            m_CurrentIndex = 0;
    }

    /// <summary>
    /// ホワイトアウト
    /// </summary>
    public void WhiteOut()
    {
        MessageBroker.Default.Publish(new WhiteOutMessage(FADE_SPEED, FADE_TIME));
    }
}
