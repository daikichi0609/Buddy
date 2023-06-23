using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UniRx;
using Zenject;
using UnityEditor.EditorTools;
using Fungus;
using Task = System.Threading.Tasks.Task;

public interface ISceneInitializer
{
    Task FungusMethod();
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
    protected CurrentCharacterHolder m_CurrentCharacterHolder;
    [Inject]
    protected IInstantiater m_Instantiater;

    private static IInjector ms_OutGameUnitInjector = new OutGameUnitInjector();

    protected virtual string FungusMessage { get; }

    /// <summary>
    /// リーダーインスタンス
    /// </summary>
    protected ICollector m_Leader;

    /// <summary>
    /// バディインスタンス
    /// </summary>
    protected ICollector m_Friend;

    protected virtual void Awake()
    {
        m_LoopManager.GetInitEvent.SubscribeWithState(this, async (_, self) => await self.OnStart()).AddTo(this);
    }

    /// <summary>
    /// スタート処理
    /// </summary>
    protected abstract Task OnStart();

    /// <summary>
    /// 移動後イベント
    /// </summary>
    protected virtual Task OnTurnBright() { return Task.CompletedTask; }

    /// <summary>
    /// Fungusから呼ぶメソッド
    /// </summary>
    public virtual Task FungusMethod() { return Task.CompletedTask; }

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
        var l = m_Instantiater.InstantiatePrefab(leader.OutGamePrefab, ms_OutGameUnitInjector);
        l.transform.position = leaderPos;
        m_Leader = l.GetComponent<ActorComponentCollector>();
        m_Leader.Initialize();

        // バディ
        var friend = m_CurrentCharacterHolder.Friend;
        var f = m_Instantiater.InstantiatePrefab(friend.OutGamePrefab, ms_OutGameUnitInjector);
        f.transform.position = friendPos;
        m_Friend = f.GetComponent<ActorComponentCollector>();
        m_Friend.Initialize();
        // ---------- //
    }

    /// <summary>
    /// 操作許可
    /// </summary>
    /// <param name="chara"></param>
    /// <param name="wrapPos"></param>
    /// <param name="cameraHandler"></param>
    protected static void AllowOperation(ICollector chara, Vector3 wrapPos, ICameraHandler cameraHandler)
    {
        var controller = chara.GetInterface<ICharaController>();
        controller.Wrap(wrapPos);
        IOutGamePlayerInput leader = chara.GetInterface<IOutGamePlayerInput>();
        leader.CanOperate = true; // 操作可能

        // カメラを追従させる
        cameraHandler.SetParent(controller.MoveObject);
    }

    /// <summary>
    /// 会話フローを持たせる
    /// </summary>
    /// <param name="chara"></param>
    /// <param name="flowchart"></param>
    /// <param name="pos"></param>
    /// <param name="conversationManager"></param>
    protected static void SetTalkFlow(ICollector chara, Flowchart flowchart, Vector3 pos, IConversationManager conversationManager)
    {
        // キャラを定位置に固定
        var friendConroller = chara.GetInterface<ICharaController>();
        friendConroller.Wrap(pos);
        friendConroller.Rigidbody.constraints = RigidbodyConstraints.FreezeAll; // 位置固定

        // 会話フローを持たせる
        var friendTalk = chara.GetInterface<ICharaTalk>();
        friendTalk.FlowChart = flowchart;
        conversationManager.Register(friendTalk);
    }
}
