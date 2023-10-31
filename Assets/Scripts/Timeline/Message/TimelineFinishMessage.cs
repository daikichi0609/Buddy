using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 次のタイムライン再生
/// </summary>
public readonly struct FinishTimelineNextTimelineMessage
{
    /// <summary>
    /// 終了したタイムライン
    /// </summary>
    public TIMELINE_TYPE Type { get; }

    /// <summary>
    /// 次のタイムライン
    /// </summary>
    public TIMELINE_TYPE NextTimeline { get; }

    public FinishTimelineNextTimelineMessage(TIMELINE_TYPE type, TIMELINE_TYPE next)
    {
        Type = type;
        NextTimeline = next;
    }
}

/// <summary>
/// Fungus再生
/// </summary>
public readonly struct FinishTimelineNextFungusMessage
{
    /// <summary>
    /// 終了したタイムライン
    /// </summary>
    public TIMELINE_TYPE Type { get; }

    /// <summary>
    /// 次のFungus
    /// </summary>
    public GameObject FungusObject { get; }

    /// <summary>
    /// リーダー座標
    /// </summary>
    public Vector3 PlayerPos { get; }

    /// <summary>
    /// フレンド座標
    /// </summary>
    public Vector3 FriendPos { get; }

    public FinishTimelineNextFungusMessage(TIMELINE_TYPE type, GameObject fungus, Vector3 playerPos, Vector3 friendPos)
    {
        Type = type;
        FungusObject = fungus;
        PlayerPos = playerPos;
        FriendPos = friendPos;
    }
}

/// <summary>
/// 操作可能
/// </summary>
public readonly struct FinishTimelineBePlayableMessage
{
    /// <summary>
    /// 終了したタイムライン
    /// </summary>
    public TIMELINE_TYPE Type { get; }

    public FinishTimelineBePlayableMessage(TIMELINE_TYPE type)
    {
        Type = type;
    }
}

/// <summary>
/// シーンをロードする
/// </summary>
public readonly struct FinishTimelineNextLoadScene
{
    /// <summary>
    /// 終了したタイムライン
    /// </summary>
    public string SceneName { get; }

    public FinishTimelineNextLoadScene(string sceneName)
    {
        SceneName = sceneName;
    }
}