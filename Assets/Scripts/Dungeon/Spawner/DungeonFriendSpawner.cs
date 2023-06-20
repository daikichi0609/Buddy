using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;
using Zenject;

public interface IDungeonFriendSpawner
{
    /// <summary>
    /// リーダー
    /// </summary>
    /// <param name="setup"></param>
    /// <param name="pos"></param>
    Task SpawnLeader(CharacterSetup setup, Vector3 pos);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="setup"></param>
    /// <param name="pos"></param>
    Task SpawnFriend(CharacterSetup setup, Vector3 pos);
}

public class DungeonFriendSpawner : IDungeonFriendSpawner
{
    [Inject]
    private IObjectPoolController m_ObjectPoolController;
    [Inject]
    private IUnitHolder m_UnitHolder;
    [Inject]
    private ITeamStatusHolder m_TeamStatusHolder;
    [Inject]
    private IMiniMapRenderer m_MiniMapRenderer;
    [Inject]
    private ICameraHandler m_CameraHandler;

    /// <summary>
    /// リーダースポーン
    /// </summary>
    /// <param name="setup"></param>
    /// <param name="pos"></param>
    /// <returns></returns>
    Task IDungeonFriendSpawner.SpawnLeader(CharacterSetup setup, Vector3 pos)
    {
        var gameObject = m_ObjectPoolController.GetObject(setup);
        gameObject.transform.position = pos;

        // 初期化
        var leader = gameObject.GetComponent<ICollector>();
        if (leader.RequireInterface<ICharaStatus>(out var e) == true)
            if (m_TeamStatusHolder.TryGetLeaderStatus(out var status) == true)
                e.SetStatus(status);
            else
                e.SetStatus(setup as CharacterSetup);

        leader.Initialize();
        PostSpawnLeader(leader);

        // 追加
        m_UnitHolder.AddFriend(leader);

        return Task.CompletedTask;

        void PostSpawnLeader(ICollector leader)
        {
            // カメラ追従
            if (leader.RequireInterface<ICharaObjectHolder>(out var holder) == true)
            {
                var disposable = m_CameraHandler.SetParent(holder.MoveObject); // カメラをリーダーに追従させる

                // 死亡時にカメラ追従を止める
                var battle = leader.GetEvent<ICharaBattleEvent>();
                battle.OnDead.SubscribeWithState(disposable, (_, self) => self.Dispose()).AddTo(leader.Disposables);

                // その他
                leader.Disposables.Add(disposable);
            }

            // ミニマップ表示
            var move = leader.GetInterface<ICharaMove>();
            m_MiniMapRenderer.SetPlayerPos(move.Position);

            // マップ更新
            if (leader.RequireEvent<ICharaTurnEvent>(out var e) == true)
            {
                e.OnTurnEndPost.SubscribeWithState2(this, leader, (_, self, leader) =>
                {
                    var pos = leader.GetInterface<ICharaMove>().Position;
                    self.m_MiniMapRenderer.SetPlayerPos(pos);
                }).AddTo(leader.Disposables);
            }
        }
    }

    /// <summary>
    /// バディスポーン
    /// </summary>
    /// <param name="setup"></param>
    /// <param name="pos"></param>
    /// <returns></returns>
    Task IDungeonFriendSpawner.SpawnFriend(CharacterSetup setup, UnityEngine.Vector3 pos)
    {
        var gameObject = m_ObjectPoolController.GetObject(setup);
        gameObject.transform.position = pos;

        // 初期化
        var leader = gameObject.GetComponent<ICollector>();
        if (leader.RequireInterface<ICharaStatus>(out var e) == true)
            if (m_TeamStatusHolder.TryGetFriendStatus(out var status) == true)
                e.SetStatus(status);
            else
                e.SetStatus(setup as CharacterSetup);

        leader.Initialize();

        // 追加
        m_UnitHolder.AddFriend(leader);

        return Task.CompletedTask;
    }
}
