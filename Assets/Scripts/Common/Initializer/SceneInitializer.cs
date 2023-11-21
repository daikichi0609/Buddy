using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UniRx;
using Zenject;
using Fungus;
using Task = System.Threading.Tasks.Task;
using System;

public interface ISceneInitializer
{
    /// <summary>
    /// 行動可能
    /// </summary>
    void AllowOperation(bool wrap = true);

    /// <summary>
    /// リーダー移動
    /// </summary>
    /// <param name="leaderPos"></param>
    /// <param name="friendPos"></param>
    void WrapLeader(Transform leaderTransform);

    /// <summary>
    /// カメラ追従
    /// </summary>
    void SetCamera();

    /// <summary>
    /// Leaderの有効化切り替え
    /// </summary>
    /// <param name="isActivate"></param>
    /// <returns></returns>
    IDisposable SwitchLeaderActive(bool isActivate);

    /// <summary>
    /// Friendの有効化切り替え
    /// </summary>
    /// <param name="isActivate"></param>
    /// <returns></returns>
    IDisposable SwitchFriendActive(bool isActivate);

    /// <summary>
    /// Fungusから呼び出す用
    /// </summary>
    void FungusMethod();
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
    protected InGameProgressHolder m_InGameProgressHolder;
    [Inject]
    protected DungeonProgressHolder m_DungeonProgressHolder;
    [Inject]
    protected CurrentCharacterHolder m_CurrentCharacterHolder;
    [Inject]
    protected IInstantiater m_Instantiater;

    public static IInjector ms_OutGameUnitInjector = new OutGameUnitInjector();

    protected virtual string FungusMessage { get; }
    protected static readonly string ms_LoseBackMessage = "LoseBack";

    /// <summary>
    /// リーダーインスタンス
    /// </summary>
    protected ICollector m_Leader;
    protected Vector3 LeaderPos { get; set; }

    /// <summary>
    /// バディインスタンス
    /// </summary>
    protected ICollector m_Friend;

    /// <summary>
    /// スタート処理
    /// </summary>
    protected abstract Task OnStart();
    private async void Start() => await OnStart();

    /// <summary>
    /// Fungusから呼ぶメソッド
    /// </summary>
    public virtual void FungusMethod() { }

    /// <summary>
    /// アウトゲームキャラ生成
    /// </summary>
    /// <param name="leaderPos"></param>
    /// <param name="friendPos"></param>
    protected void CreateOutGameCharacter(Vector3 leaderPos, Vector3 friendPos)
    {
        // ----- キャラクター生成 ----- //
        // リーダー
        var leader = m_CurrentCharacterHolder.Leader;
        var l = m_Instantiater.InstantiatePrefab(leader.Prefab, ms_OutGameUnitInjector);
        l.transform.position = leaderPos;
        m_Leader = l.GetComponent<ActorComponentCollector>();
        m_Leader.Initialize();

        // バディ
        if (m_CurrentCharacterHolder.TryGetFriend(m_InGameProgressHolder.FriendIndex, out var friend) == true)
        {
            var f = m_Instantiater.InstantiatePrefab(friend.Prefab, ms_OutGameUnitInjector);
            f.transform.position = friendPos;
            m_Friend = f.GetComponent<ActorComponentCollector>();
            m_Friend.Initialize();
        }
        // ---------- //
    }

    /// <summary>
    /// 操作許可
    /// </summary>
    /// <param name="wrapPos"></param>
    protected void AllowOperation(bool wrap = true)
    {
        if (wrap == true)
        {
            var controller = m_Leader.GetInterface<ICharaController>();
            controller.Wrap(LeaderPos);
        }
        var input = m_Leader.GetInterface<IOutGamePlayerInput>();
        input.CanOperate = true; // 操作可能

        SetCamera();
    }
    void ISceneInitializer.AllowOperation(bool wrap) => AllowOperation(wrap);

    /// <summary>
    /// カメラ追従
    /// </summary>
    protected void SetCamera()
    {
        // カメラを追従させる
        var objectHolder = m_Leader.GetInterface<ICharaObjectHolder>();
        m_CameraHandler.SetParent(objectHolder.MoveObject);
    }
    void ISceneInitializer.SetCamera() => SetCamera();

    /// <summary>
    /// リーダーワープ
    /// </summary>
    /// <param name="leaderPos"></param>
    /// <param name="friendPos"></param>
    void ISceneInitializer.WrapLeader(Transform leaderTransform)
    {
        var leader = m_Leader.GetInterface<ICharaObjectHolder>();
        leader.MoveObject.transform.position = leaderTransform.position;
        leader.CharaObject.transform.rotation = leaderTransform.rotation;
    }

    /// <summary>
    /// 有効化切り替え
    /// </summary>
    /// <param name="isActivate"></param>
    /// <returns></returns>
    IDisposable ISceneInitializer.SwitchLeaderActive(bool isActivate)
    {
        if (m_Leader == null)
            return Disposable.Empty;

        var objectHolder = m_Leader.GetInterface<ICharaObjectHolder>();
        objectHolder.MoveObject.SetActive(isActivate);
        return Disposable.CreateWithState((objectHolder, isActivate), tuple => tuple.objectHolder.MoveObject.SetActive(!tuple.isActivate));
    }

    /// <summary>
    /// 有効化切り替え
    /// </summary>
    /// <param name="isActivate"></param>
    /// <returns></returns>
    IDisposable ISceneInitializer.SwitchFriendActive(bool isActivate)
    {
        if (m_Friend == null)
            return Disposable.Empty;

        var objectHolder = m_Friend.GetInterface<ICharaObjectHolder>();
        objectHolder.MoveObject.SetActive(isActivate);
        return Disposable.CreateWithState((objectHolder, isActivate), tuple => tuple.objectHolder.MoveObject.SetActive(!tuple.isActivate));
    }
}
