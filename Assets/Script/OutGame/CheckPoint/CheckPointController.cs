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

    /// <summary>
    /// リーダーインスタンス
    /// </summary>
    private GameObject m_Leader;

    /// <summary>
    /// バディインスタンス
    /// </summary>
    private GameObject m_Friend;

    protected override void Awake()
    {
        base.Awake();
        PlayerLoopManager.Interface.GetInitEvent.Subscribe(_ =>
        {
            OnStart();
        }).AddTo(this);
    }

    private void OnStart()
    {
        // 明転
        var currentDungeon = DungeonProgressManager.Interface.CurrentDungeonSetup;
        var dungeonName = currentDungeon.DungeonName;
        FadeManager.Interface.EndFade(() => StartTimeline(), dungeonName, CHECK_POINT);

        // ステージ生成
        var checkPoint = currentDungeon.CheckPointSetup;
        Instantiate(checkPoint.Stage);

        // キャラクター生成
        var leader = OutGameInfoHolder.Interface.Leader;
        m_Leader = Instantiate(leader.OutGamePrefab);
        m_Leader.transform.position = LEADER_START_POS;
        var friend = OutGameInfoHolder.Interface.Friend;
        m_Friend = Instantiate(friend.OutGamePrefab);
        m_Friend.transform.position = FRIEND_START_POS;
    }

    private async void StartTimeline()
    {
        // コントローラー取得
        ICharaController leader = m_Leader.GetComponent<CharaController>();
        ICharaController friend = m_Friend.GetComponent<CharaController>();

        // 定点移動
        var _ = leader.Move(LEADER_END_POS, 3f);
        await friend.Move(FRIEND_END_POS, 3f);

        // 向き合う
        leader.Face(m_Friend.transform.position);
        friend.Face(m_Leader.transform.position);
    }
}
