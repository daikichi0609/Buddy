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
    [Inject]
    private IInstantiater m_Instantiater;

    private static readonly string ms_FungusMessage = "Timeline";

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

        MessageBroker.Default.Receive<FinishTimelineBePlayableMessage>().SubscribeWithState(this, (message, self) => self.OnFinishBePlayable(message)).AddTo(this);
        MessageBroker.Default.Receive<FinishTimelineNextTimelineMessage>().SubscribeWithState(this, (message, self) => self.OnFinishNextTimeline(message)).AddTo(this);
        MessageBroker.Default.Receive<FinishTimelineNextFungusMessage>().SubscribeWithState(this, (message, self) => self.OnFinishNextFungus(message)).AddTo(this);
    }

    /// <summary>
    /// タイムライン再生
    /// </summary>
    /// <param name="type"></param>
    /// <param name="finish"></param>
    private void Play(TIMELINE_TYPE type, bool finish = false)
    {
        if (m_CurrentDirector.TryGetValue(type, out var register) == false)
        {
#if DEBUG
            Debug.LogWarning("タイムラインが未登録です。" + type.ToString());
#endif
            return;
        }

        m_FadeManager.StartFade((this, register, finish), tuple =>
        {
            if (tuple.finish == true)
                tuple.Item1.FinishInternal();
            tuple.Item1.PlayInternal(tuple.register);
        });
    }
    void ITimelineManager.Play(TIMELINE_TYPE type) => Play(type);

    /// <summary>
    /// タイムライン再生
    /// </summary>
    /// <param name="register"></param>
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

    /// <summary>
    /// 操作可能
    /// </summary>
    /// <param name="message"></param>
    private void OnFinishBePlayable(FinishTimelineBePlayableMessage message)
    {
        if (m_CurrentDirector.TryGetValue(message.Type, out var register) == false)
        {
#if DEBUG
            Debug.LogWarning("タイムラインが未登録です。" + message.Type.ToString());
#endif
            return;
        }

        m_FadeManager.StartFade(this, self =>
        {
            self.FinishInternal();
            self.m_SceneInitializer.AllowOperation();
        });
    }

    /// <summary>
    /// 続けてタイムライン再生
    /// </summary>
    /// <param name="message"></param>
    private void OnFinishNextTimeline(FinishTimelineNextTimelineMessage message)
    {
        if (m_CurrentDirector.TryGetValue(message.Type, out var register) == false)
        {
#if DEBUG
            Debug.LogWarning("タイムラインが未登録です。" + message.Type.ToString());
#endif
            return;
        }

        Play(message.NextTimeline, true);
    }

    /// <summary>
    /// 続けてFungus
    /// </summary>
    /// <param name="message"></param>
    private void OnFinishNextFungus(FinishTimelineNextFungusMessage message)
    {
        if (m_CurrentDirector.TryGetValue(message.Type, out var register) == false)
        {
#if DEBUG
            Debug.LogWarning("タイムラインが未登録です。" + message.Type.ToString());
#endif
            return;
        }

        m_FadeManager.StartFade((this, message), tuple =>
        {
            tuple.Item1.FinishInternal();
            tuple.Item1.m_SceneInitializer.FaceEachOther(tuple.message.PlayerPos, tuple.message.FriendPos);
        },
        (this, message), tuple =>
        {
            var fungus = tuple.Item1.m_Instantiater.InstantiatePrefab(tuple.message.FungusObject).GetComponent<Fungus.Flowchart>();
            fungus.SendFungusMessage(ms_FungusMessage);
        });
    }

    /// <summary>
    /// タイムライン再生終了
    /// </summary>
    private void FinishInternal()
    {
        m_OnFinish.Clear();
    }
}