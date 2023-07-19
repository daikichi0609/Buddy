using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System.Threading.Tasks;
using Zenject;

public class CheckPointInitializer : SceneInitializer
{
    [Inject]
    private IConversationManager m_ConversationManager;

    protected override string FungusMessage => "CheckPoint";

    private Vector3 LeaderStartPos { get; set; }
    private Vector3 FriendStartPos { get; set; }
    private Vector3 LeaderEndPos { get; set; }
    private Vector3 FriendEndPos { get; set; }

    private Vector3 FriendPos { get; set; }

    /// <summary>
    /// Fungusフロー
    /// </summary>
    private Fungus.Flowchart m_ArrivalFlowChart;
    private Fungus.Flowchart m_DeparturedFlowChart;

    private void Awake()
    {
        MessageBroker.Default.Receive<CheckPointInitializerInfo>().SubscribeWithState(this, (info, self) =>
        {
            self.LeaderStartPos = info.LeaderStartPos;
            self.FriendStartPos = info.FriendStartPos;
            self.LeaderEndPos = info.LeaderEndPos;
            self.FriendEndPos = info.FriendEndPos;

            self.LeaderPos = info.LeaderPos;
            self.FriendPos = info.FriendPos;
        }).AddTo(this);
    }

    /// <summary>
    /// スタート処理
    /// </summary>
    protected override async Task OnStart()
    {
        var currentDungeon = m_DungeonProgressHolder.CurrentDungeonSetup;
        var checkPoint = currentDungeon.CheckPointSetup;
        m_Instantiater.InstantiatePrefab(checkPoint.Stage); // ステージ生成

        CreateOutGameCharacter(LeaderStartPos, FriendStartPos); // キャラクター生成

        // 会話フロー生成
        m_ArrivalFlowChart = m_Instantiater.InstantiatePrefab(checkPoint.ArrivalFlow).GetComponent<Fungus.Flowchart>();
        m_DeparturedFlowChart = m_Instantiater.InstantiatePrefab(checkPoint.DepartureFlow).GetComponent<Fungus.Flowchart>();

        // 明転
        await m_FadeManager.TurnBright(this, async self => await self.OnTurnBright(), checkPoint.CheckPointName, "チェックポイント");
    }

    /// <summary>
    /// 会話開始前
    /// </summary>
    private async Task OnTurnBright()
    {
        // コントローラー取得
        ICharaController leader = m_Leader.GetInterface<ICharaController>();
        ICharaController friend = m_Friend.GetInterface<ICharaController>();

        // 定点移動
        var leaderMove = leader.MoveToPoint(LeaderEndPos, 3f);
        var friendMove = friend.MoveToPoint(FriendEndPos, 3f);
        await Task.WhenAll(leaderMove, friendMove);
        await Task.Delay(500);

        // 向き合う
        leader.Face(friend.Position);
        friend.Face(leader.Position);

        // 会話開始
        m_ArrivalFlowChart.SendFungusMessage(FungusMessage);
    }

    /// <summary>
    /// 操作可能にする
    /// </summary>
    public override void FungusMethod()
    {
        m_FadeManager.StartFade(this, self =>
        {
            self.AllowOperation();
            self.m_ConversationManager.Register(self.m_Friend, self.m_DeparturedFlowChart, self.FriendPos);
        });
    }
}
