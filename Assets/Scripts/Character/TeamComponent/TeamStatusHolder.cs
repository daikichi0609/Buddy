using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITeamStatusHolder
{
    /// <summary>
    /// ステータス保存
    /// </summary>
    /// <param name="leaderStatus"></param>
    void RegisterLeaderStatus(CurrentStatus leaderStatus);

    /// <summary>
    /// ステータス保存
    /// </summary>
    /// <param name="friendStatus"></param>
    void RegisterFriendStatus(CurrentStatus friendStatus);

    /// <summary>
    /// リーダーステータス取得
    /// </summary>
    /// <param name="status"></param>
    /// <returns></returns>
    bool TryGetLeaderStatus(out CurrentStatus status);

    /// <summary>
    /// バディステータス取得
    /// </summary>
    /// <param name="status"></param>
    /// <returns></returns>
    bool TryGetFriendStatus(out CurrentStatus status);
}

public class TeamStatusHolder : ITeamStatusHolder
{
    /// <summary>
    /// リーダーステータス
    /// </summary>
    private CurrentStatus m_LeaderStatus;

    /// <summary>
    /// バディステータス
    /// </summary>
    private CurrentStatus m_FriendStatus;

    /// <summary>
    /// ステータス登録
    /// </summary>
    /// <param name="leaderStatus"></param>
    void ITeamStatusHolder.RegisterLeaderStatus(CurrentStatus leaderStatus) => m_LeaderStatus = leaderStatus;
    void ITeamStatusHolder.RegisterFriendStatus(CurrentStatus friendStatus) => m_FriendStatus = friendStatus;

    /// <summary>
    /// リーダーステータス取得
    /// </summary>
    /// <param name="status"></param>
    /// <returns></returns>
    bool ITeamStatusHolder.TryGetLeaderStatus(out CurrentStatus status)
    {
        status = m_LeaderStatus;
        return status != null;
    }

    /// <summary>
    /// バディステータス取得
    /// </summary>
    /// <param name="status"></param>
    /// <returns></returns>
    bool ITeamStatusHolder.TryGetFriendStatus(out CurrentStatus status)
    {
        status = m_FriendStatus;
        return status != null;
    }
}
