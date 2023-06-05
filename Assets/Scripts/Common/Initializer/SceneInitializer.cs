using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UniRx;
using Zenject;
using UnityEditor.EditorTools;

public interface ISceneInitializer
{
    Task ReadyToOperatable();
}

public abstract class SceneInitializer : MonoBehaviour, ISceneInitializer
{
    [Inject]
    protected IPlayerLoopManager m_LoopManager;
    [Inject]
    protected ICameraHandler m_CameraHandler;
    [Inject]
    protected IBGMHandler m_BGMHandler;
    [Inject]
    protected IFadeManager m_FadeManager;
    [Inject]
    protected DungeonProgressHolder m_DungeonProgressHolder;
    [Inject]
    protected OutGameInfoHolder m_OutGameInfoHolder;
    [Inject]
    protected IInstantiater m_Instantiater;

    protected virtual string FungusMessage { get; }

    protected static readonly float OFFSET_Y = 0.5f;

    protected virtual Vector3 LeaderStartPos { get; }
    protected virtual Vector3 LeaderEndPos { get; }

    protected virtual Vector3 FriendStartPos { get; }
    protected virtual Vector3 FriendEndPos { get; }

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

    private void Awake()
    {
        m_LoopManager.GetInitEvent.Subscribe(async _ => await OnStart()).AddTo(this);
    }

    /// <summary>
    /// スタート処理
    /// </summary>
    protected virtual Task OnStart()
    {
        // ----- キャラクター生成 ----- //
        // リーダー
        var leader = m_OutGameInfoHolder.Leader;
        var l = m_Instantiater.InstantiatePrefab(leader.OutGamePrefab);
        l.transform.position = LeaderStartPos;
        m_Leader = l.GetComponent<ActorComponentCollector>();
        m_Leader.Initialize();

        // バディ
        var friend = m_OutGameInfoHolder.Friend;
        var f = m_Instantiater.InstantiatePrefab(friend.OutGamePrefab);
        f.transform.position = FriendStartPos;
        m_Friend = f.GetComponent<ActorComponentCollector>();
        m_Friend.Initialize();
        // ---------- //

        return Task.CompletedTask;
    }

    /// <summary>
    /// 移動後イベント
    /// </summary>
    protected virtual Task OnTurnBright() { return default; }

    /// <summary>
    /// 操作可能にする
    /// </summary>
    public virtual Task ReadyToOperatable() { return default; }
}
