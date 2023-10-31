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
    FINAL_INTRO = 4,
    FINAL_KING = 5,
}

public class TimelineRegister : MonoBehaviour
{
    private enum TIMELINE_FINISH_TYPE
    {
        PLAYER_PLAYABLE,
        NEXT_TIMELINE,
        FUNGUS_EVENT,
        LOAD_SCENE,
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
    [SerializeField, Header("次に再生するFungus")]
    private GameObject m_Fungus;

    [BoxGroup("終了イベント")]
    [SerializeField, Header("シーン名")]
    private string m_SceneName;

    [BoxGroup("終了イベント")]
    [SerializeField, Header("リーダー座標")]
    private Transform m_LeaderPos;

    [BoxGroup("終了イベント")]
    [SerializeField, Header("フレンド座標")]
    private Transform m_FriendPos;

    [BoxGroup("終了イベント・固有")]
    [SerializeField, Header("ラゴン座標")]
    private bool m_DeployRagon;
    [BoxGroup("終了イベント・固有")]
    [SerializeField, Header("ラゴン座標"), ShowIf("m_DeployRagon")]
    private Transform m_RagonTransform;

    [BoxGroup("終了イベント・固有")]
    [SerializeField, Header("ベリィ座標")]
    private bool m_DeployBerry;
    [BoxGroup("終了イベント・固有")]
    [SerializeField, Header("ベリィ座標"), ShowIf("m_DeployBerry")]
    private Transform m_BerryTransform;

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
                DeployTimelineCharacter();
                MessageBroker.Default.Publish(new FinishTimelineNextFungusMessage(m_Type, m_Fungus, m_LeaderPos.position, m_FriendPos.position));
                break;

            case TIMELINE_FINISH_TYPE.LOAD_SCENE:
                MessageBroker.Default.Publish(new FinishTimelineNextLoadScene(m_SceneName));
                break;
        }
    }

    private void DeployTimelineCharacter()
    {
        if (m_DeployRagon == true)
            MessageBroker.Default.Publish(new DeployTimelineCharacterMessage(CHARA_NAME.RAGON, m_RagonTransform));
        if (m_DeployBerry == true)
            MessageBroker.Default.Publish(new DeployTimelineCharacterMessage(CHARA_NAME.BERRY, m_BerryTransform));
    }
}
