using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System.Threading.Tasks;

public class CheckPointInitializer : SceneInitializer<CheckPointInitializer>
{
    protected override string FungusMessage => "CheckPoint";

    protected override Vector3 LeaderStartPos => new Vector3(-1f, OFFSET_Y, -5f);
    protected override Vector3 LeaderEndPos => new Vector3(-1f, OFFSET_Y, -1.5f);

    protected override Vector3 FriendStartPos => new Vector3(1f, OFFSET_Y, -5f);
    protected override Vector3 FriendEndPos => new Vector3(1f, OFFSET_Y, 0f);

    private static readonly Vector3 FRIEND_POS = new Vector3(1f, OFFSET_Y, 7.5f);

    /// <summary>
    /// Fungusフロー
    /// </summary>
    private Fungus.Flowchart m_DeparturedFlowChart;

    /// <summary>
    /// スタート処理
    /// </summary>
    protected override void OnStart()
    {
        var currentDungeon = DungeonProgressManager.Interface.CurrentDungeonSetup;
        var checkPoint = currentDungeon.CheckPointSetup;

        // 明転
        FadeManager.Interface.TurnBright(() => _ = OnTurnBright(), checkPoint.CheckPointName, "チェックポイント");

        // ステージ生成
        Instantiate(checkPoint.Stage);

        // 会話フロー生成
        m_ArrivalFlowChart = Instantiate(checkPoint.ArrivalFlow).GetComponent<Fungus.Flowchart>();
        m_DeparturedFlowChart = Instantiate(checkPoint.DepartureFlow).GetComponent<Fungus.Flowchart>();

        base.OnStart();
    }

    /// <summary>
    /// 会話開始前
    /// </summary>
    async protected override Task OnTurnBright()
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
        leader.Face(friend.Position - leader.Position);
        friend.Face(leader.Position - friend.Position);

        // 会話開始
        m_ArrivalFlowChart.SendFungusMessage(FungusMessage);
    }

    /// <summary>
    /// 操作可能にする
    /// </summary>
    public override void ReadyToOperatable()
    {
        // リーダー
        var leaderController = m_Leader.GetInterface<ICharaController>();
        leaderController.Wrap(new Vector3(0f, OFFSET_Y, 0f));
        IOutGamePlayerInput leader = m_Leader.GetInterface<IOutGamePlayerInput>();
        leader.CanOperate = true; // 操作可能

        // バディ
        var friendConroller = m_Friend.GetInterface<ICharaController>();
        friendConroller.Wrap(FRIEND_POS);
        friendConroller.Rigidbody.constraints = RigidbodyConstraints.FreezeAll; // 位置固定

        // バディに会話フローを持たせる
        var friendTalk = m_Friend.GetInterface<ICharaTalk>();
        friendTalk.FlowChart = m_DeparturedFlowChart;
        ConversationManager.Interface.Register(friendTalk);

        // カメラをリーダーに追従させる
        CameraHandler.Interface.SetParent(leaderController.MoveObject);
    }
}
