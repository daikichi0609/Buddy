using System.Collections.Generic;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;
using Zenject;

public interface ITimelineManager
{
    /// <summary>
    /// 再生
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    bool Play(TIMELINE_TYPE type);

    /// <summary>
    /// 停止
    /// </summary>
    void Finish();
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
    [Inject]
    private IInputManager m_InputManager;

    /// <summary>
    /// タイムラインキャラ
    /// </summary>
    private TimelineCharacterHolder m_TimelineCharacters;
    private List<DeployTimelineCharacterMessage> m_DeployTimelineMessages = new List<DeployTimelineCharacterMessage>();
    private CompositeDisposable m_ResetDeploy = new CompositeDisposable();

    private static readonly string ms_FungusMessage = "Timeline";

    /// <summary>
    /// 現在再生中のタイムライン
    /// </summary>
    private Dictionary<TIMELINE_TYPE, RegisterTimelineMessage> m_CurrentDirector = new Dictionary<TIMELINE_TYPE, RegisterTimelineMessage>();

    /// <summary>
    /// タイムライン再生終了中
    /// </summary>
    private bool m_WhileFinishing;

    /// <summary>
    /// タイムライン終了時
    /// </summary>
    private CompositeDisposable m_OnFinish = new CompositeDisposable();

    private void Awake()
    {
        // タイムライン登録
        MessageBroker.Default.Receive<RegisterTimelineMessage>().SubscribeWithState(this, (message, self) => self.m_CurrentDirector.Add(message.Key, message)).AddTo(this);

        // アウトゲームキャラ操作可能
        MessageBroker.Default.Receive<FinishTimelineBePlayableMessage>().SubscribeWithState(this, (message, self) => self.OnFinishBePlayable(message)).AddTo(this);
        // 続けてタイムライン再生
        MessageBroker.Default.Receive<FinishTimelineNextTimelineMessage>().SubscribeWithState(this, (message, self) => self.OnFinishNextTimeline(message)).AddTo(this);
        // 続けてFungus
        MessageBroker.Default.Receive<FinishTimelineNextFungusMessage>().SubscribeWithState(this, (message, self) => self.OnFinishNextFungus(message)).AddTo(this);
        // ロードシーン
        MessageBroker.Default.Receive<FinishTimelineNextSceneLoadMessage>().SubscribeWithState(this, (message, self) => self.OnFinishNextScene(message)).AddTo(this);
        // ボスバトル開始
        MessageBroker.Default.Receive<FinishTimelineReadyToBossBattleMessage>().SubscribeWithState(this, (message, self) => self.OnFinishReadyToBossBattle(message)).AddTo(this);
        // ゲームクリア
        MessageBroker.Default.Receive<FinishTimelineGameClearMessage>().SubscribeWithState(this, (message, self) => self.OnFinishGameClear(message)).AddTo(this);

        MessageBroker.Default.Receive<DeployTimelineCharacterMessage>().SubscribeWithState(this, (message, self) => self.m_DeployTimelineMessages.Add(message)).AddTo(this);
        MessageBroker.Default.Receive<TimelineCharacterMessage>().SubscribeWithState(this, (message, self) => self.m_TimelineCharacters = message.TimelineCharacterHolder).AddTo(this);
        MessageBroker.Default.Receive<ResetTimelineCharacterMessage>().SubscribeWithState(this, (_, self) => self.ResetTimelineCharacter()).AddTo(this);
    }

    /// <summary>
    /// タイムライン再生
    /// </summary>
    /// <param name="type"></param>
    /// <param name="finish">連続再生</param>
    private bool Play(TIMELINE_TYPE type, bool finish = false)
    {
        if (m_CurrentDirector.TryGetValue(type, out var register) == false)
        {
#if DEBUG
            Debug.LogWarning("タイムラインが未登録です。" + type.ToString());
#endif
            return false;
        }

        m_FadeManager.StartFade((this, register, finish), tuple =>
        {
            if (tuple.finish == true)
                tuple.Item1.OnFinish();
            tuple.Item1.PlayInternal(tuple.register);
        });
        return true;
    }
    bool ITimelineManager.Play(TIMELINE_TYPE type) => Play(type);

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

        // クロップ有効化
        MessageBroker.Default.Publish(new CropSetActivateMessage(true));
        var crop = Disposable.Create(() => MessageBroker.Default.Publish(new CropSetActivateMessage(false)));
        m_OnFinish.Add(crop);

        // 再生停止
        var finish = Disposable.CreateWithState(register, register => register.Director.Stop());
        m_OnFinish.Add(finish);

        // タイムラインスキップ
        var input = m_InputManager.InputStartEvent.SubscribeWithState(register, (info, register) =>
        {
            if (info.KeyCodeFlag.HasBitFlag(KeyCodeFlag.Return) == true)
                register.FinishTimeline();
        });
        m_OnFinish.Add(input);

        // CinemachineTrackの状態をResetする
        register.Director.Stop();
        register.Director.Play();
    }

    /// <summary>
    /// タイムライン再生終了
    /// </summary>
    private void OnFinish()
    {
        m_OnFinish.Clear();
        m_WhileFinishing = false;
    }
    void ITimelineManager.Finish() => OnFinish();

    /// <summary>
    /// タイムライン終了可能確認
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private bool CheckCanFinish(TIMELINE_TYPE type)
    {
        if (m_CurrentDirector.TryGetValue(type, out var register) == false)
        {
#if DEBUG
            Debug.LogWarning("タイムラインが未登録です。" + type.ToString());
#endif
            return false;
        }

        if (m_WhileFinishing == true)
            return false;

        m_WhileFinishing = true;
        return true;
    }

    /// <summary>
    /// 操作可能
    /// </summary>
    /// <param name="message"></param>
    private void OnFinishBePlayable(FinishTimelineBePlayableMessage message)
    {
        if (CheckCanFinish(message.Type) == false)
            return;

        m_FadeManager.StartFade(this, self =>
        {
            self.OnFinish();
            self.m_SceneInitializer.AllowOperation();
        });
    }

    /// <summary>
    /// 続けてタイムライン再生
    /// </summary>
    /// <param name="message"></param>
    private void OnFinishNextTimeline(FinishTimelineNextTimelineMessage message)
    {
        if (CheckCanFinish(message.Type) == false)
            return;

        Play(message.NextTimeline, true);
    }

    /// <summary>
    /// 続けてFungus
    /// </summary>
    /// <param name="message"></param>
    private void OnFinishNextFungus(FinishTimelineNextFungusMessage message)
    {
        if (CheckCanFinish(message.Type) == false)
            return;

        m_FadeManager.StartFade((this, message), async tuple =>
        {
            tuple.Item1.OnFinish();
            await Task.Delay(1); // TL終了処理待ちで1F挟む
            tuple.Item1.m_SceneInitializer.WrapLeaderAndFriend(message.PlayerTransform, message.FriendTransform);
            tuple.Item1.m_SceneInitializer.SetCamera();
            tuple.Item1.DeployTimelineCharacter();
        },
        (this, message), tuple =>
        {
            var fungus = tuple.Item1.m_Instantiater.InstantiatePrefab(tuple.message.FungusObject).GetComponent<Fungus.Flowchart>();
            fungus.SendFungusMessage(ms_FungusMessage);
        });
    }

    /// <summary>
    /// 続けてロードシーン
    /// </summary>
    /// <param name="message"></param>
    private void OnFinishNextScene(FinishTimelineNextSceneLoadMessage message)
    {
        if (CheckCanFinish(message.Type) == false)
            return;

        m_FadeManager.LoadScene(message.SceneName.GetSceneName());
    }

    /// <summary>
    /// ボスバトル開始
    /// </summary>
    /// <param name="message"></param>
    private void OnFinishReadyToBossBattle(FinishTimelineReadyToBossBattleMessage message)
    {
        if (CheckCanFinish(message.Type) == false)
            return;

        MessageBroker.Default.Publish(new ReadyToBossBattleMessage());
    }

    /// <summary>
    /// ゲームクリア
    /// </summary>
    /// <param name="message"></param>
    private void OnFinishGameClear(FinishTimelineGameClearMessage message)
    {
        if (CheckCanFinish(message.Type) == false)
            return;

        MessageBroker.Default.Publish(new GameClearMessage());
    }

    /// <summary>
    /// タイムラインキャラ配置
    /// </summary>
    private void DeployTimelineCharacter()
    {
        foreach (var m in m_DeployTimelineMessages)
        {
            GameObject chara = m.CharaName switch
            {
                CHARA_NAME.BOXMAN => m_TimelineCharacters.Logue,
                CHARA_NAME.RAGON => m_TimelineCharacters.Ragon,
                CHARA_NAME.BERRY => m_TimelineCharacters.Berry,
                CHARA_NAME.DORCHE => m_TimelineCharacters.Dorch,
                CHARA_NAME.BALE => m_TimelineCharacters.Bale,
                CHARA_NAME.LAMY => m_TimelineCharacters.Lamy,
                CHARA_NAME.PLISS => m_TimelineCharacters.Plis,
                _ => null
            };
            chara.SetActive(true);
            chara.transform.position = m.Transform.position;
            chara.transform.rotation = m.Transform.rotation;

            var reset = Disposable.CreateWithState(chara, chara => chara.SetActive(false));
            m_ResetDeploy.Add(reset);
        }
        m_DeployTimelineMessages.Clear();
    }

    /// <summary>
    /// タイムラインキャラ配置リセット
    /// </summary>
    private void ResetTimelineCharacter()
    {
        m_ResetDeploy.Clear();
    }
}