using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UniRx;

public interface IPlayerInput : ICharacterComponent
{
}

/// <summary>
/// 入力機能
/// </summary>
public class PlayerInput : CharaComponentBase, IPlayerInput
{
    private ICharaBattle m_CharaBattle;
    private ICharaMove m_CharaMove;
    private ICharaTurn m_CharaTurn;

    protected override void Register(ICollector owner)
    {
        base.Register(owner);
        owner.Register<IPlayerInput>(this);
    }

    protected override void Initialize()
    {
        base.Initialize();
        CharaUiManager.Interface.InitializeCharacterUi(this.Owner);

        m_CharaBattle = Owner.GetComponent<ICharaBattle>();
        m_CharaMove = Owner.GetComponent<ICharaMove>();
        m_CharaTurn = Owner.GetComponent<ICharaTurn>();

        InputManager.Interface.InputEvent.Subscribe(input =>
        {
            DetectInput(input.KeyCodeFlag);
        }).AddTo(this);
    }

    /// <summary>
    /// 攻撃、移動
    /// </summary>
    /// <param name="flag"></param>
    private void DetectInput(KeyCodeFlag flag)
    {
        // 行動許可ないなら何もしない。行動中なら何もしない。
        if (m_CharaTurn.CanAct == false || m_CharaTurn.IsActing == true)
            return;

        // Ui操作中なら何もしない
        if (InputManager.Interface.IsUiPopUp == true)
            return;

        // 攻撃
        if (DetectInputAttack(flag) == true)
            return;

        // 移動
        if (DetectInputMove(flag) == true)
            return;
    }

    /// <summary>
    /// 攻撃
    /// </summary>
    /// <param name="flag"></param>
    /// <returns></returns>
    private bool DetectInputAttack(KeyCodeFlag flag)
    {
        if (flag.HasBitFlag(KeyCodeFlag.E))
        {
            m_CharaBattle.NormalAttack();
            return true;
        }

        return false;
    }

    /// <summary>
    /// 移動
    /// </summary>
    /// <param name="flag"></param>
    /// <returns></returns>
    private bool DetectInputMove(KeyCodeFlag flag)
    {
        var direction = new Vector3();

        if (flag.HasBitFlag(KeyCodeFlag.W))
            direction += new Vector3(0f, 0f, 1f);

        if (flag.HasBitFlag(KeyCodeFlag.A))
            direction += new Vector3(-1f, 0f, 0f);

        if (flag.HasBitFlag(KeyCodeFlag.S))
            direction += new Vector3(0f, 0f, -1);

        if (flag.HasBitFlag(KeyCodeFlag.D))
            direction += new Vector3(1f, 0f, 0f);

        // 入力なし
        if (direction == new Vector3(0f, 0f, 0f))
            return false;

        // 斜め入力限定
        bool diagonal = flag.HasBitFlag(KeyCodeFlag.Right_Shift);
        if (diagonal == true && JudgeDirectionDiagonal(direction) == false)
            return false;

        m_CharaMove.Move(direction);
        return true;
    }

    /// <summary>
    /// 入力方向が斜めかを見る
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    private bool JudgeDirectionDiagonal(Vector3 direction)
    {
        if (direction == new Vector3(1f, 0f, 1f))
            return true;

        if (direction == new Vector3(1f, 0f, -1f))
            return true;

        if (direction == new Vector3(-1f, 0f, 1f))
            return true;

        if (direction == new Vector3(-1f, 0f, -1f))
            return true;

        return false;
    }
}