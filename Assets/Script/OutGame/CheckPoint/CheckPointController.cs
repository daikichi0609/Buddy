using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class CheckPointController : Singleton<CheckPointController>
{
    private static readonly string CHECK_POINT = "チェックポイント";
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
    private GameObject m_FungusFlow;

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
        // 明転
        var currentDungeon = DungeonProgressManager.Interface.CurrentDungeonSetup;
        var dungeonName = currentDungeon.DungeonName;
        FadeManager.Interface.EndFade(() => StartTimeline(), dungeonName, CHECK_POINT);

        // ステージ生成
        var checkPoint = currentDungeon.CheckPointSetup;
        Instantiate(checkPoint.Stage);

        // 会話フロー生成
        m_FungusFlow = Instantiate(checkPoint.FungusFlow);

        // ----- キャラクター生成 ----- //
        // リーダー
        var leader = OutGameInfoHolder.Interface.Leader;
        var l = Instantiate(leader.OutGamePrefab);
        l.transform.position = LEADER_START_POS;
        m_Leader = l.GetComponent<ActorComponentCollector>();

        // バディ
        var friend = OutGameInfoHolder.Interface.Friend;
        var f = Instantiate(friend.OutGamePrefab);
        f.transform.position = FRIEND_START_POS;
        m_Friend = f.GetComponent<ActorComponentCollector>();
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
        var _ = leader.Move(LEADER_END_POS, 3f);
        await friend.Move(FRIEND_END_POS, 3f);

        // 向き合う
        leader.Face(friend.Position);
        friend.Face(friend.Position);

        // 会話開始
        m_FungusFlow.SetActive(true);
    }

    /// <summary>
    /// 会話終了後
    /// </summary>
    public void OnEndDialog() => FadeManager.Interface.StartFade(() => ReadyToPlayable(), string.Empty, string.Empty);

    /// <summary>
    /// 操作可能にする
    /// </summary>
    private void ReadyToPlayable()
    {
        // リーダー
        m_Leader.GetInterface<ICharaController>().Wrap(new Vector3(0f, OFFSET_Y, 0f));
        IOutGamePlayerInput leader = m_Leader.GetInterface<IOutGamePlayerInput>();
        leader.CanOperate = true;

        // バディ
        m_Friend.GetInterface<ICharaController>().Wrap(FRIEND_POS);
    }
}
