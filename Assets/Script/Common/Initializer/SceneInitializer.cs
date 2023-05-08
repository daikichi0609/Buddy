using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UniRx;

public abstract class SceneInitializer<T> : Singleton<T> where T : MonoBehaviour
{
    protected abstract string FungusMessage { get; }

    protected static readonly float OFFSET_Y = 0.5f;

    protected abstract Vector3 LeaderStartPos { get; }
    protected abstract Vector3 LeaderEndPos { get; }

    protected abstract Vector3 FriendStartPos { get; }
    protected abstract Vector3 FriendEndPos { get; }

    /// <summary>
    /// リーダーインスタンス
    /// </summary>
    protected ICollector m_Leader;

    /// <summary>
    /// バディインスタンス
    /// </summary>
    protected ICollector m_Friend;

    /// <summary>
    /// 到着時会話フロー
    /// </summary>
    protected Fungus.Flowchart m_ArrivalFlowChart;

    protected override void Awake()
    {
        base.Awake();
        PlayerLoopManager.Interface.GetInitEvent.Subscribe(_ => OnStart()).AddTo(this);
    }

    /// <summary>
    /// スタート処理
    /// </summary>
    protected virtual void OnStart()
    {
        // ----- キャラクター生成 ----- //
        // リーダー
        var leader = OutGameInfoHolder.Interface.Leader;
        var l = Instantiate(leader.OutGamePrefab);
        l.transform.position = LeaderStartPos;
        m_Leader = l.GetComponent<ActorComponentCollector>();
        m_Leader.Initialize();

        // バディ
        var friend = OutGameInfoHolder.Interface.Friend;
        var f = Instantiate(friend.OutGamePrefab);
        f.transform.position = FriendStartPos;
        m_Friend = f.GetComponent<ActorComponentCollector>();
        m_Friend.Initialize();
        // ---------- //
    }

    /// <summary>
    /// 移動後イベント
    /// </summary>
    protected abstract Task OnTurnBright();

    /// <summary>
    /// 操作可能にする
    /// </summary>
    public abstract void ReadyToOperatable();
}
