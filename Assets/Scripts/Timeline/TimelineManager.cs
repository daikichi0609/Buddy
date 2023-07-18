using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Zenject;

public interface ITimelineManager
{
    void Play(TIMELINE_TYPE type);
}

public class TimelineManager : MonoBehaviour, ITimelineManager
{
    [Inject]
    private ICameraHandler m_CameraHandler;
    [Inject]
    private IFadeManager m_FadeManager;
    [Inject]
    private ISceneInitializer m_SceneInitializer;

    [SerializeField]
    private GameObject m_TimelineCamera;

    /// <summary>
    /// 現在再生中のタイムライン
    /// </summary>
    private Dictionary<TIMELINE_TYPE, PlayableDirector> m_CurrentDirector = new Dictionary<TIMELINE_TYPE, PlayableDirector>();

    /// <summary>
    /// カメラ切り替え
    /// </summary>
    private IDisposable m_SwitchCamera;

    private void Awake()
    {
        MessageBroker.Default.Receive<RegisterTimelineMessage>().SubscribeWithState(this, (info, self) => self.m_CurrentDirector.Add(info.Key, info.Director)).AddTo(this);
        MessageBroker.Default.Receive<FinishTimelineMessage>().SubscribeWithState(this, (info, self) => self.Finish(info)).AddTo(this);
    }

    void ITimelineManager.Play(TIMELINE_TYPE type)
    {
        if (m_CurrentDirector.TryGetValue(type, out var director) == false)
        {
#if DEBUG
            Debug.LogWarning("タイムラインが未登録です。" + type.ToString());
#endif
            return;
        }

        m_FadeManager.StartFade((this, director), tuple => tuple.Item1.PlayInternal(tuple.director));
    }

    private void PlayInternal(PlayableDirector director)
    {
        m_SwitchCamera = m_CameraHandler.SetActive(false);
        m_TimelineCamera.SetActive(true);

        TimelineAsset timelineAsset = director.playableAsset as TimelineAsset;
        director.SetGenericBinding(timelineAsset.GetOutputTrack(0), m_TimelineCamera);
        // CinemachineTrackの状態をResetする
        director.Stop();
        director.Play();
    }

    private void Finish(FinishTimelineMessage message)
    {
        if (m_CurrentDirector.TryGetValue(message.Type, out var director) == false)
        {
#if DEBUG
            Debug.LogWarning("タイムラインが未登録です。" + message.Type.ToString());
#endif
            return;
        }
        switch (message.FinishType)
        {
            case TIMELINE_FINISH_TYPE.PLAYABLE:
                m_FadeManager.StartFade((this, director), tuple =>
                {
                    tuple.Item1.FinishInternal(director);
                    tuple.Item1.m_SceneInitializer.AllowOperation();
                });
                break;
        }
    }

    private void FinishInternal(PlayableDirector director)
    {
        m_TimelineCamera.SetActive(false);
        m_SwitchCamera?.Dispose();

        TimelineAsset timelineAsset = director.playableAsset as TimelineAsset;
        director.ClearGenericBinding(timelineAsset.GetOutputTrack(0));
    }
}