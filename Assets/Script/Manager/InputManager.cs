using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;

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

	// インベントリ
	Q = 1 << 7,

	// Ui決定
	Return = 1 << 8,
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

public interface IInputManager : ISingleton
{
    IObservable<InputInfo> InputEvent { get; }
	Action SetActiveUi(IUiBase ui);
	bool IsUiPopUp { get; }
}

public class InputManager : Singleton<InputManager, IInputManager>, IInputManager
{
	/// <summary>
	/// 入力イベント
	/// </summary>
	private Subject<InputInfo> m_InputEvent = new Subject<InputInfo>();
	IObservable<InputInfo> IInputManager.InputEvent => m_InputEvent;

	/// <summary>
	/// 今表示中のUi
	/// </summary>
	private IUiBase ActiveUi { get; set; }

    Action IInputManager.SetActiveUi(IUiBase ui)
	{
		if (IsUiPopUp == true)
			return null;

		ActiveUi = ui;
		return () => ActiveUi = null;
	}

	/// <summary>
	/// UI表示中かどうか
	/// </summary>
	public bool IsUiPopUp => ActiveUi != null;

	protected override void Awake()
    {
        base.Awake();

		GameManager.Interface.GetUpdateEvent
			.Subscribe(_ => DetectInput()).AddTo(this);
    }

    //入力を見てメッセージ発行
    private void DetectInput()
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

        m_InputEvent.OnNext(new InputInfo(flag));
	}
}

// 拡張メソッド
static class EnumExtensions
{
    public static bool HasBitFlag(this KeyCodeFlag value, KeyCodeFlag flag) => (value & flag) == flag;
}