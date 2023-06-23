﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using Zenject;

[Flags]
public enum KeyCodeFlag
{
    None = 0,

    // ----- 移動 ----- //
    W = 1 << 1,
    A = 1 << 2,
    S = 1 << 3,
    D = 1 << 4,
    Right_Shift = 1 << 5,

    // 攻撃
    E = 1 << 6,

    // 戻る
    Q = 1 << 7,

    // メニュー
    M = 1 << 8,

    // Ui決定
    Return = 1 << 9,
}

public readonly struct InputInfo
{
    /// <summary>
    /// キーコード
    /// </summary>
    public KeyCodeFlag KeyCodeFlag { get; }

    public InputInfo(KeyCodeFlag flag)
    {
        KeyCodeFlag = flag;
    }
}

public interface IInputManager
{
    IObservable<InputInfo> InputEvent { get; }
    IObservable<InputInfo> InputStartEvent { get; }

    IDisposable SetActiveUi(IUiBase ui);
    bool IsUiPopUp { get; }
}

public class InputManager : MonoBehaviour, IInputManager
{
    /// <summary>
    /// 入力イベント
    /// </summary>
    private Subject<InputInfo> m_InputEvent = new Subject<InputInfo>();
    IObservable<InputInfo> IInputManager.InputEvent => m_InputEvent;

    /// <summary>
    /// 入力始めイベント
    /// </summary>
    private Subject<InputInfo> m_InputStartEvent = new Subject<InputInfo>();
    IObservable<InputInfo> IInputManager.InputStartEvent => m_InputStartEvent;

    /// <summary>
    /// 今表示中のUi
    /// </summary>
    private IUiBase m_ActiveUi;

    /// <summary>
    /// UI表示中かどうか
    /// </summary>
    public bool IsUiPopUp => m_ActiveUi != null;

    /// <summary>
    /// 操作中Uiをセット
    /// </summary>
    /// <param name="ui"></param>
    /// <returns></returns>
    IDisposable IInputManager.SetActiveUi(IUiBase ui)
    {
        m_ActiveUi = ui;
        return Disposable.CreateWithState(this, self => self.m_ActiveUi = null);
    }

    [Inject]
    public void Construct(IPlayerLoopManager loopManager)
    {
        loopManager.GetUpdateEvent.SubscribeWithState(this, (_, self) => self.DetectInput());
    }

    //入力を見てメッセージ発行
    private void DetectInput()
    {
        var input = CreateGetKeyFlag();
        var start = CreateGetKeyDownFlag();

        m_InputEvent.OnNext(new InputInfo(input));
        m_InputStartEvent.OnNext(new InputInfo(start));
    }

    /// <summary>
    /// 入力中ずっと
    /// </summary>
    /// <returns></returns>
	private KeyCodeFlag CreateGetKeyFlag()
    {
        var flag = KeyCodeFlag.None;

        if (Input.GetKey(KeyCode.W))
            flag |= KeyCodeFlag.W;

        if (Input.GetKey(KeyCode.A))
            flag |= KeyCodeFlag.A;

        if (Input.GetKey(KeyCode.S))
            flag |= KeyCodeFlag.S;

        if (Input.GetKey(KeyCode.D))
            flag |= KeyCodeFlag.D;

        if (Input.GetKey(KeyCode.RightShift))
            flag |= KeyCodeFlag.Right_Shift;

        if (Input.GetKey(KeyCode.E))
            flag |= KeyCodeFlag.E;

        if (Input.GetKey(KeyCode.Q))
            flag |= KeyCodeFlag.Q;

        if (Input.GetKey(KeyCode.M))
            flag |= KeyCodeFlag.M;

        if (Input.GetKey(KeyCode.Return))
            flag |= KeyCodeFlag.Return;

        return flag;
    }

    /// <summary>
    /// 入力時
    /// </summary>
    /// <returns></returns>
    private KeyCodeFlag CreateGetKeyDownFlag()
    {
        var flag = KeyCodeFlag.None;

        if (Input.GetKeyDown(KeyCode.W))
            flag |= KeyCodeFlag.W;

        if (Input.GetKeyDown(KeyCode.A))
            flag |= KeyCodeFlag.A;

        if (Input.GetKeyDown(KeyCode.S))
            flag |= KeyCodeFlag.S;

        if (Input.GetKeyDown(KeyCode.D))
            flag |= KeyCodeFlag.D;

        if (Input.GetKeyDown(KeyCode.RightShift))
            flag |= KeyCodeFlag.Right_Shift;

        if (Input.GetKeyDown(KeyCode.E))
            flag |= KeyCodeFlag.E;

        if (Input.GetKeyDown(KeyCode.Q))
            flag |= KeyCodeFlag.Q;

        if (Input.GetKeyDown(KeyCode.M))
            flag |= KeyCodeFlag.M;

        if (Input.GetKeyDown(KeyCode.Return))
            flag |= KeyCodeFlag.Return;

        return flag;
    }
}

// 拡張メソッド
static class EnumExtensions
{
    public static bool HasBitFlag(this KeyCodeFlag value, KeyCodeFlag flag) => (value & flag) == flag;
}