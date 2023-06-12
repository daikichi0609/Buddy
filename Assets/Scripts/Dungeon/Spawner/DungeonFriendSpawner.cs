using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        // 追加
        m_UnitHolder.AddFriend(leader);

        return Task.CompletedTask;
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
