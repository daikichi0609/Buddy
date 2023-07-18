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

    /// <summary>
    /// 現在再生中のタイムライン
    /// </summary>
    private Dictionary<TIMELINE_TYPE, RegisterTimelineMessage> m_CurrentDirector = new Dictionary<TIMELINE_TYPE, RegisterTimelineMessage>();

    /// <summary>
    /// タイムライン終了時
    /// </summary>
    private CompositeDisposable m_OnFinish = new CompositeDisposable();

    private void Awake()
    {
        MessageBroker.Default.Receive<RegisterTimelineMessage>().SubscribeWithState(this, (message, self) => self.m_CurrentDirector.Add(message.Key, message)).AddTo(this);
        MessageBroker.Default.Receive<FinishTimelineMessage>().SubscribeWithState(this, (message, self) => self.Finish(message)).AddTo(this);
    }

    void ITimelineManager.Play(TIMELINE_TYPE type)
    {
        if (m_CurrentDirector.TryGetValue(type, out var register) == false)
        {
#if DEBUG
            Debug.LogWarning("タイムラインが未登録です。" + type.ToString());
#endif
            return;
        }

        m_FadeManager.StartFade((this, register), tuple => tuple.Item1.PlayInternal(tuple.register));
    }

    private void PlayInternal(RegisterTimelineMessage register)
    {
        // メインカメラオフ
        var mainCamera = m_CameraHandler.SetActive(false);
        m_OnFinish.Add(mainCamera);

        // タイムライン用カメラオン
        register.Camera.SetActive(true);
        var camera = Disposable.CreateWithState(register.Camera, camera => camera.SetActive(false));
        m_OnFinish.Add(camera);

        // リーダー無効化
        var leader = m_SceneInitializer.SwitchLeaderActive(false);
        m_OnFinish.Add(leader);

        // フレンド無効化
        var friend = m_SceneInitializer.SwitchFriendActive(false);
        m_OnFinish.Add(friend);

        // CinemachineTrackの状態をResetする
        register.Director.Stop();
        register.Director.Play();
    }

    private void Finish(FinishTimelineMessage message)
    {
        if (m_CurrentDirector.TryGetValue(message.Type, out var register) == false)
        {
#if DEBUG
            Debug.LogWarning("タイムラインが未登録です。" + message.Type.ToString());
#endif
            return;
        }
        switch (message.FinishType)
        {
            case TIMELINE_FINISH_TYPE.PLAYABLE:
                m_FadeManager.StartFade(this, self =>
                {
                    self.FinishInternal();
                    self.m_SceneInitializer.AllowOperation();
                });
                break;
        }
    }

    private void FinishInternal()
    {
        m_OnFinish.Clear();
    }
}