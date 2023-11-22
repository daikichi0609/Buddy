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
    public Transform PlayerTransform { get; }

    /// <summary>
    /// フレンド座標
    /// </summary>
    public Transform FriendTransform { get; }

    public FinishTimelineNextFungusMessage(TIMELINE_TYPE type, GameObject fungus, Transform playerTransform, Transform friendTransform)
    {
        Type = type;
        FungusObject = fungus;
        PlayerTransform = playerTransform;
        FriendTransform = friendTransform;
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
public readonly struct FinishTimelineNextSceneLoadMessage
{
    /// <summary>
    /// 終了したタイムライン
    /// </summary>
    public TIMELINE_TYPE Type { get; }

    /// <summary>
    /// 読み込むシーンの名前
    /// </summary>
    public SceneName.SCENE_NAME SceneName { get; }

    public FinishTimelineNextSceneLoadMessage(TIMELINE_TYPE type, SceneName.SCENE_NAME name)
    {
        Type = type;
        SceneName = name;
    }
}

/// <summary>
/// ボスバトル開始
/// </summary>
public readonly struct FinishTimelineReadyToBossBattleMessage
{
    /// <summary>
    /// 終了したタイムライン
    /// </summary>
    public TIMELINE_TYPE Type { get; }

    public FinishTimelineReadyToBossBattleMessage(TIMELINE_TYPE type) => Type = type;
}

/// <summary>
/// ゲームクリア
/// </summary>
public readonly struct FinishTimelineGameClearMessage
{
    /// <summary>
    /// 終了したタイムライン
    /// </summary>
    public TIMELINE_TYPE Type { get; }

    public FinishTimelineGameClearMessage(TIMELINE_TYPE type) => Type = type;
}