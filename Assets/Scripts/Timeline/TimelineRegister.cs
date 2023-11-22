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
    KING_INTRO = 5,
    KING_HELPER = 6,
    KING_DEFEAT = 7,
    BARM_INTRO = 8,
    BARM_DEFEAT = 9,
}

public class TimelineRegister : MonoBehaviour
{
    private enum TIMELINE_FINISH_TYPE
    {
        PLAYER_PLAYABLE,
        NEXT_TIMELINE,
        FUNGUS_EVENT,
        LOAD_SCENE,
        READY_TO_BATTLE,
        GAME_CLEAR,
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

    [BoxGroup("終了イベント・タイプ")]
    [SerializeField, Header("再生終了後イベント")]
    private TIMELINE_FINISH_TYPE m_FinishType;

    [BoxGroup("終了イベント・Timeline")]
    [SerializeField, Header("次に再生するタイムライン")]
    private TIMELINE_TYPE m_NextTimeline;

    [BoxGroup("終了イベント・Fungus")]
    [SerializeField, Header("次に再生するFungus")]
    private GameObject m_Fungus;

    [BoxGroup("終了イベント・ロードシーン")]
    [SerializeField, Header("シーン名")]
    private SceneName.SCENE_NAME m_SceneName;

    [BoxGroup("終了イベント・座標")]
    [SerializeField, Header("リーダー座標")]
    private Transform m_LeaderTransform;

    [BoxGroup("終了イベント・座標")]
    [SerializeField, Header("フレンド座標")]
    private Transform m_FriendTransform;

    [BoxGroup("終了イベント・座標")]
    [SerializeField, Header("ラゴン座標")]
    private bool m_DeployRagon;
    [BoxGroup("終了イベント・座標")]
    [SerializeField, Header("ラゴン座標"), ShowIf("m_DeployRagon")]
    private Transform m_RagonTransform;

    [BoxGroup("終了イベント・座標")]
    [SerializeField, Header("ベリィ座標")]
    private bool m_DeployBerry;
    [BoxGroup("終了イベント・座標")]
    [SerializeField, Header("ベリィ座標"), ShowIf("m_DeployBerry")]
    private Transform m_BerryTransform;

    [BoxGroup("終了イベント・座標")]
    [SerializeField, Header("ドルケ座標")]
    private bool m_DeployDorch;
    [BoxGroup("終了イベント・座標")]
    [SerializeField, Header("ドルケ座標"), ShowIf("m_DeployDorch")]
    private Transform m_DorchTransform;

    [BoxGroup("終了イベント・座標")]
    [SerializeField, Header("ベイル座標")]
    private bool m_DeployBale;
    [BoxGroup("終了イベント・座標")]
    [SerializeField, Header("ベイル座標"), ShowIf("m_DeployBale")]
    private Transform m_BaleTransform;

    [BoxGroup("終了イベント・座標")]
    [SerializeField, Header("ラミィ座標")]
    private bool m_DeployLamy;
    [BoxGroup("終了イベント・座標")]
    [SerializeField, Header("ラミィ座標"), ShowIf("m_DeployLamy")]
    private Transform m_LamyTransform;

    [BoxGroup("終了イベント・座標")]
    [SerializeField, Header("プリス座標")]
    private bool m_DeployPlis;
    [BoxGroup("終了イベント・座標")]
    [SerializeField, Header("プリス座標"), ShowIf("m_DeployPlis")]
    private Transform m_PlisTransform;

    private static readonly float FADE_SPEED = 0.5f;
    private static readonly float FADE_TIME = 1f;

    private void Awake()
    {
        m_Camera.SetActive(false);
        MessageBroker.Default.Publish(new RegisterTimelineMessage(m_Camera, m_Director, m_Type, this, self => self.OnFinish()));
    }

    private void DeployTimelineCharacter()
    {
        if (m_DeployRagon == true)
            MessageBroker.Default.Publish(new DeployTimelineCharacterMessage(CHARA_NAME.RAGON, m_RagonTransform));
        if (m_DeployBerry == true)
            MessageBroker.Default.Publish(new DeployTimelineCharacterMessage(CHARA_NAME.BERRY, m_BerryTransform));
        if (m_DeployDorch == true)
            MessageBroker.Default.Publish(new DeployTimelineCharacterMessage(CHARA_NAME.DORCHE, m_DorchTransform));
        if (m_DeployBale == true)
            MessageBroker.Default.Publish(new DeployTimelineCharacterMessage(CHARA_NAME.BALE, m_BaleTransform));
        if (m_DeployLamy == true)
            MessageBroker.Default.Publish(new DeployTimelineCharacterMessage(CHARA_NAME.LAMY, m_LamyTransform));
        if (m_DeployPlis == true)
            MessageBroker.Default.Publish(new DeployTimelineCharacterMessage(CHARA_NAME.PLISS, m_PlisTransform));
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
                MessageBroker.Default.Publish(new FinishTimelineNextFungusMessage(m_Type, m_Fungus, m_LeaderTransform, m_FriendTransform));
                break;

            case TIMELINE_FINISH_TYPE.LOAD_SCENE:
                MessageBroker.Default.Publish(new FinishTimelineNextSceneLoadMessage(m_Type, m_SceneName));
                break;

            case TIMELINE_FINISH_TYPE.READY_TO_BATTLE:
                MessageBroker.Default.Publish(new FinishTimelineReadyToBossBattleMessage(m_Type));
                break;

            case TIMELINE_FINISH_TYPE.GAME_CLEAR:
                MessageBroker.Default.Publish(new FinishTimelineGameClearMessage(m_Type));
                break;
        }
    }
}
