using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITeamStatusHolder
{
    /// <summary>
    /// ステータス保存
    /// </summary>
    /// <param name="leaderStatus"></param>
    /// <param name="friendStatus"></param>
    void RegisterStatus(CurrentStatus leaderStatus, CurrentStatus friendStatus);

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
    private CurrentStatus m_FrinedStatus;

    /// <summary>
    /// ステータス登録
    /// </summary>
    /// <param name="leaderStatus"></param>
    /// <param name="friendStatus"></param>
    void ITeamStatusHolder.RegisterStatus(CurrentStatus leaderStatus, CurrentStatus friendStatus)
    {
        m_LeaderStatus = leaderStatus;
        m_FrinedStatus = friendStatus;
    }

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
        status = m_FrinedStatus;
        return status != null;
    }
}
