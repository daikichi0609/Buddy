using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System.Threading.Tasks;

public class CheckPointController : Singleton<CheckPointController>
{
    private static readonly string CHECK_POINT = "CheckPoint";

    private static readonly float OFFSET_Y = 0.5f;

    private static readonly Vector3 LEADER_START_POS = new Vector3(-1f, OFFSET_Y, -5f);
    private static readonly Vector3 LEADER_END_POS = new Vector3(-1f, OFFSET_Y, -1.5f);

    private static readonly Vector3 FRIEND_START_POS = new Vector3(1f, OFFSET_Y, -5f);
    private static readonly Vector3 FRIEND_END_POS = new Vector3(1f, OFFSET_Y, 0f);

    private static readonly Vector3 FRIEND_POS = new Vector3(1f, OFFSET_Y, 7.5f);

    /// <summary>
    /// リーダーインスタンス
    /// </summary>
    private ICollector m_Leader;

    /// <summary>
    /// バディインスタンス
    /// </summary>
    private ICollector m_Friend;

    /// <summary>
    /// Fungusフロー
    /// </summary>
    private Fungus.Flowchart m_ArrivalFlowChart;
    private Fungus.Flowchart m_DeparturedFlowChart;

    protected override void Awake()
    {
        base.Awake();
        PlayerLoopManager.Interface.GetInitEvent.Subscribe(_ =>
        {
            OnStart();
        }).AddTo(this);
    }

    /// <summary>
    /// スタート処理
    /// </summary>
    private void OnStart()
    {
        var currentDungeon = DungeonProgressManager.Interface.CurrentDungeonSetup;
        var checkPoint = currentDungeon.CheckPointSetup;

        // 明転
        FadeManager.Interface.EndFade(() => StartTimeline(), checkPoint.CheckPointName, "チェックポイント");

        // ステージ生成
        Instantiate(checkPoint.Stage);

        // 会話フロー生成
        m_ArrivalFlowChart = Instantiate(checkPoint.ArrivalFlow).GetComponent<Fungus.Flowchart>();
        m_DeparturedFlowChart = Instantiate(checkPoint.DepartureFlow).GetComponent<Fungus.Flowchart>();

        // ----- キャラクター生成 ----- //
        // リーダー
        var leader = OutGameInfoHolder.Interface.Leader;
        var l = Instantiate(leader.OutGamePrefab);
        l.transform.position = LEADER_START_POS;
        m_Leader = l.GetComponent<ActorComponentCollector>();
        m_Leader.Initialize();

        // バディ
        var friend = OutGameInfoHolder.Interface.Friend;
        var f = Instantiate(friend.OutGamePrefab);
        f.transform.position = FRIEND_START_POS;
        m_Friend = f.GetComponent<ActorComponentCollector>();
        m_Friend.Initialize();
        // ---------- //
    }

    /// <summary>
    /// 会話開始前
    /// </summary>
    private async void StartTimeline()
    {
        // コントローラー取得
        ICharaController leader = m_Leader.GetInterface<ICharaController>();
        ICharaController friend = m_Friend.GetInterface<ICharaController>();

        // 定点移動
        var leaderMove = leader.MoveToPoint(LEADER_END_POS, 3f);
        var friendMove = friend.MoveToPoint(FRIEND_END_POS, 3f);
        await Task.WhenAll(leaderMove, friendMove);

        // 向き合う
        leader.Face(friend.Position - leader.Position);
        friend.Face(leader.Position - friend.Position);

        // 会話開始
        m_ArrivalFlowChart.SendFungusMessage(CHECK_POINT);
    }

    /// <summary>
    /// 操作可能にする
    /// </summary>
    public void ReadyToOperatable()
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
