using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

/// <summary>
/// タイムライン登録
/// </summary>
public readonly struct RegisterTimelineMessage
{
    public GameObject Camera { get; }
    public PlayableDirector Director { get; }
    public TIMELINE_TYPE Key { get; }
    private TimelineRegister TimelineRegister { get; }
    private Action<TimelineRegister> OnFinish { get; }

    public RegisterTimelineMessage(GameObject camera, PlayableDirector director, TIMELINE_TYPE key, TimelineRegister register, Action<TimelineRegister> onFinish)
    {
        Camera = camera;
        Director = director;
        Key = key;
        TimelineRegister = register;
        OnFinish = onFinish;
    }

    public void FinishTimeline() => OnFinish(TimelineRegister);
}

/// <summary>
/// クロップテキスト表示
/// </summary>
public readonly struct CropTextMessage
{
    /// <summary>
    /// 表示時間
    /// </summary>
    public double Duration { get; }

    /// <summary>
    /// テキスト
    /// </summary>
    public string Text { get; }

    public CropTextMessage(double duration, string text)
    {
        Duration = duration;
        Text = text;
    }
}

/// <summary>
/// クロップ有効化
/// </summary>
public readonly struct CropSetActivateMessage
{
    /// <summary>
    /// 有効化
    /// </summary>
    public bool IsActive { get; }

    public CropSetActivateMessage(bool isActive) => IsActive = isActive;
}

/// <summary>
/// ホワイトアウト
/// </summary>
public readonly struct WhiteOutMessage
{
    public float Speed { get; }
    public float Time { get; }

    public WhiteOutMessage(float speed, float time)
    {
        Speed = speed;
        Time = time;
    }
}

/// <summary>
/// キャラ配置
/// </summary>
public readonly struct DeployTimelineCharacterMessage
{
    public CHARA_NAME CharaName { get; }
    public Transform Transform { get; }

    public DeployTimelineCharacterMessage(CHARA_NAME name, Transform tr)
    {
        CharaName = name;
        Transform = tr;
    }
}

/// <summary>
/// キャラ配置リセット
/// </summary>
public readonly struct ResetTimelineCharacterMessage
{

}