using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System.Threading.Tasks;
using Zenject;
using Fungus;
using Task = System.Threading.Tasks.Task;
using System;

public class HomeInitializer : SceneInitializer
{
    [Inject]
    private IConversationManager m_ConversationManager;
    [Inject]
    private HomeSetup m_HomeSetup;
    [Inject]
    private ITimelineManager m_TimelineManager;
    [Inject]
    private IHomeCharacterManager m_HomeCharacterManager;

    protected override string FungusMessage => "Home";

    private Vector3 FriendPos { get; set; }

    private IDisposable m_HomeCharactersActivate;

    private Fungus.Flowchart m_DeparturedFlowChart;
    private Fungus.Flowchart m_LoseBackFlowChart;

    private static readonly Vector3 DEFAULT_EULERANGLE = new Vector3(0f, 180f, 0f);

    private void Awake()
    {
        MessageBroker.Default.Receive<HomeInitializerInfo>().SubscribeWithState(this, (info, self) =>
        {
            self.LeaderPos = info.LeaderTransform.position;
            self.FriendPos = info.FriendTransform.position;
        }).AddTo(this);
    }

    /// <summary>
    /// スタート処理
    /// </summary>
    protected override async Task OnStart()
    {
        // ステージ生成
        Instantiate(m_HomeSetup.Stage);
        // キャラ生成
        CreateOutGameCharacter(LeaderPos, FriendPos);
        // カメラセット
        SetCamera();

        // BGM
        var bgm = Instantiate(m_HomeSetup.BGM);
        m_BGMHandler.SetBGM(bgm);

        // フロー作成＆会話登録
        if (m_HomeSetup.TryGetFriendFlow(m_InGameProgressHolder.FriendIndex, out var fFlow) == true)
        {
            var go = m_Instantiater.InstantiatePrefab(fFlow);
            m_DeparturedFlowChart = go.GetComponent<Fungus.Flowchart>();
            m_ConversationManager.Register(m_Friend, m_DeparturedFlowChart, FriendPos);
        }

        m_LoseBackFlowChart = m_Instantiater.InstantiatePrefab(m_HomeSetup.LoseBackFlow).GetComponent<Fungus.Flowchart>();

        // 攻略済みフローセット
        for (int i = 0; i < m_InGameProgressHolder.FriendIndex; i++)
        {
            if (m_HomeSetup.TryGetFriendCompletedFlow(i, out var go) == true)
            {
                // フロー取得
                var flow = m_Instantiater.InstantiatePrefab(go).GetComponent<Fungus.Flowchart>();

                var chara = m_HomeCharacterManager.GetHomeCharacter(i);
                var talk = chara.GetInterface<ICharaTalk>();
                talk.FlowChart = flow; // フロー置き換え
            }
        }

        // 進行度最大
        if (m_InGameProgressHolder.IsMaxInGameProgress == true)
        {
            m_InGameProgressHolder.NoFriend = true;
            m_TimelineManager.Play(TIMELINE_TYPE.FINAL_INTRO);

            m_HomeCharactersActivate = m_HomeCharacterManager.DeactivateAll(); // ホームキャラ非表示
        }
        // 初回タイムライン再生が終わっていない
        else if (m_InGameProgressHolder.CurrentIntroCompleted == false)
        {
            var theme = m_InGameProgressHolder.CurrentIntroTimelineTheme;
            if (m_TimelineManager.Play(theme) == true)
                m_InGameProgressHolder.CurrentIntroCompleted = true; // フラグオン

            m_HomeCharactersActivate = m_HomeCharacterManager.DeactivateAll(); // ホームキャラ非表示
        }
        // 通常時
        else if (m_InGameProgressHolder.LoseBack == false)
            await m_FadeManager.TurnBright(this, self => self.AllowOperation());
        // 負け帰ったとき
        else
        {
            m_InGameProgressHolder.LoseBack = false;
            m_LoseBackFlowChart.SendFungusMessage(ms_LoseBackMessage);
        }
    }

    /// <summary>
    /// タイムライン → Fungus終了時
    /// </summary>
    public override void FungusMethod()
    {
        m_FadeManager.StartFade(this, self =>
        {
            var fObject = self.m_Friend.GetInterface<ICharaObjectHolder>();
            fObject.MoveObject.transform.position = FriendPos; // 元の位置に戻す
            fObject.CharaObject.transform.eulerAngles = DEFAULT_EULERANGLE;

            MessageBroker.Default.Publish(new ResetTimelineCharacterMessage());

            if (self.m_HomeCharactersActivate != null)
            {
                self.m_HomeCharactersActivate.Dispose();
                self.m_HomeCharactersActivate = null;
            }
        },
        this, self => self.AllowOperation(false));
    }
}
