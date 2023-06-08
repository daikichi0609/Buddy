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
    private HomeSetup m_HomeSetup;
    [Inject]
    private IConversationManager m_ConversationManager;

    protected override string FungusMessage => "Home";

    private Vector3 LeaderPos { get; set; }
    private Vector3 FriendPos { get; set; }

    private Fungus.Flowchart m_DeparturedFlowChart;

    protected override void Awake()
    {
        base.Awake();

        MessageBroker.Default.Receive<HomeInitializerInfo>().Subscribe(info =>
        {
            LeaderPos = info.LeaderPos;
            FriendPos = info.FriendPos;
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

        var leader = m_Leader.GetInterface<ICharaController>().MoveObject;
        leader.GetComponent<Rigidbody>().useGravity = true;

        m_DeparturedFlowChart = m_Instantiater.InstantiatePrefab(m_HomeSetup.FriendFlow).GetComponent<Fungus.Flowchart>();

        AllowOperation(m_Leader, LeaderPos, m_CameraHandler);
        SetTalkFlow(m_Friend, m_DeparturedFlowChart, FriendPos, m_ConversationManager);

        // 仮
        await m_FadeManager.TurnBright(async () => await OnTurnBright(), "", "");
    }
}
