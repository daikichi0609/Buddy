using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System.Threading.Tasks;
using Zenject;
using Fungus;
using Task = System.Threading.Tasks.Task;

public class HomeInitializer : SceneInitializer
{
    [Inject]
    private IConversationManager m_ConversationManager;
    [Inject]
    private HomeSetup m_HomeSetup;
    [Inject]
    private ITimelineManager m_TimelineManager;

    protected override string FungusMessage => "Home";

    private Vector3 FriendPos { get; set; }

    private Fungus.Flowchart m_DeparturedFlowChart;

    private void Awake()
    {
        MessageBroker.Default.Receive<HomeInitializerInfo>().SubscribeWithState(this, (info, self) =>
        {
            self.LeaderPos = info.LeaderPos;
            self.FriendPos = info.FriendPos;
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

        // BGM
        var bgm = Instantiate(m_HomeSetup.BGM);
        m_BGMHandler.SetBGM(bgm);

        m_DeparturedFlowChart = m_Instantiater.InstantiatePrefab(m_HomeSetup.GetFriendFlow(m_InGameProgressHolder.Progress)).GetComponent<Fungus.Flowchart>();
        m_ConversationManager.Register(m_Friend, m_DeparturedFlowChart, FriendPos);

        int progress = m_InGameProgressHolder.Progress;
        if (m_InGameProgressHolder.IsCompletedIntro[progress] == false)
            m_TimelineManager.Play((TIMELINE_TYPE)progress);
        else
            await m_FadeManager.TurnBright(this, self => self.AllowOperation());
    }
}
