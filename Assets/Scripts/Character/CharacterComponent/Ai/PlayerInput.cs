using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UniRx;
using System.Threading.Tasks;
using Zenject;
using System;

public interface IPlayerInput : IActorInterface
{
    void DetectInput();
}

/// <summary>
/// 入力機能
/// </summary>
public class PlayerInput : ActorComponentBase, IPlayerInput
{
    [Inject]
    private IInputManager m_InputManager;

    private ICharaBattle m_CharaBattle;
    private ICharaMove m_CharaMove;
    private ICharaTurn m_CharaTurn;
    private ICharaLastActionHolder m_LastActionHolder;

    protected override void Register(ICollector owner)
    {
        base.Register(owner);
        owner.Register<IPlayerInput>(this);
    }

    protected override void Initialize()
    {
        base.Initialize();

        m_CharaBattle = Owner.GetInterface<ICharaBattle>();
        m_CharaMove = Owner.GetInterface<ICharaMove>();
        m_CharaTurn = Owner.GetInterface<ICharaTurn>();
        m_LastActionHolder = Owner.GetInterface<ICharaLastActionHolder>();
    }

    /// <summary>
    /// 入力検知
    /// </summary>
    private async void DetectInput()
    {
        // すでに行動済みなら再帰抜ける
        if (m_LastActionHolder.LastAction != CHARA_ACTION.NONE)
            return;

        // プレイヤーの入力結果を見る
        var flag = m_InputManager.InputKeyCode;
        var result = await DetectInputInternal(flag);

        // 入力結果が有効ならターン終了
        if (result == true)
            await m_CharaTurn.TurnEnd();
        else
        {
            await Task.Delay(1);
            DetectInput();
        }
    }
    void IPlayerInput.DetectInput() => DetectInput();

    /// <summary>
    /// 入力検知 攻撃、移動
    /// </summary>
    /// <param name="flag"></param>
    private async Task<bool> DetectInputInternal(KeyCodeFlag flag)
    {
        // 行動許可ないなら何もしない。行動中なら何もしない。
        if (m_CharaTurn.IsActing == true)
            return false;

        // Ui操作中なら再帰終了
        if (m_InputManager.IsUiPopUp == true)
            return false;

        // 移動
        if (await DetectInputMove(flag) == true)
            return true;

        // 攻撃
        if (await DetectInputAttack(flag) == true)
            return true;

        return false;
    }

    /// <summary>
    /// 攻撃
    /// </summary>
    /// <param name="flag"></param>
    /// <returns></returns>
    private async Task<bool> DetectInputAttack(KeyCodeFlag flag)
    {
        if (flag.HasBitFlag(KeyCodeFlag.E))
            return await m_CharaBattle.NormalAttack();

        return false;
    }

    /// <summary>
    /// 移動
    /// </summary>
    /// <param name="flag"></param>
    /// <returns></returns>
    private async Task<bool> DetectInputMove(KeyCodeFlag flag)
    {
        var direction = new Vector3Int();

        if (flag.HasBitFlag(KeyCodeFlag.W))
            direction += new Vector3Int(0, 0, 1);

        if (flag.HasBitFlag(KeyCodeFlag.A))
            direction += new Vector3Int(-1, 0, 0);

        if (flag.HasBitFlag(KeyCodeFlag.S))
            direction += new Vector3Int(0, 0, -1);

        if (flag.HasBitFlag(KeyCodeFlag.D))
            direction += new Vector3Int(1, 0, 0);

        // 入力なし
        if (direction == new Vector3Int(0, 0, 0))
            return false;

        // 方向転換だけ
        bool onlyFace = flag.HasBitFlag(KeyCodeFlag.Right_Shift);

        var dir = direction.ToDirEnum();
        if (onlyFace == true)
        {
            m_CharaMove.Face(dir);
            return false; // ターン消費しない
        }

        return await m_CharaMove.Move(dir);
    }
}