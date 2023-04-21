using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UniRx;
using System.Threading.Tasks;

public interface IPlayerInput : IActorInterface
{
}

/// <summary>
/// 入力機能
/// </summary>
public class PlayerInput : ActorComponentBase, IPlayerInput
{
    private ICharaBattle m_CharaBattle;
    private ICharaMove m_CharaMove;
    private ICharaTurn m_CharaTurn;
    private ICharaLastActionHolder m_CharaLastAction;

    protected override void Register(ICollector owner)
    {
        base.Register(owner);
        owner.Register<IPlayerInput>(this);
    }

    protected override void Initialize()
    {
        base.Initialize();
        CharaUiManager.Interface.InitializeCharacterUi(this.Owner);

        m_CharaBattle = Owner.GetInterface<ICharaBattle>();
        m_CharaMove = Owner.GetInterface<ICharaMove>();
        m_CharaTurn = Owner.GetInterface<ICharaTurn>();
        m_CharaLastAction = Owner.GetInterface<ICharaLastActionHolder>();

        InputManager.Interface.InputEvent.Subscribe(input =>
        {
            DetectInput(input.KeyCodeFlag);
        }).AddTo(CompositeDisposable);
    }

    /// <summary>
    /// 入力検知
    /// </summary>
    private void DetectInput(KeyCodeFlag flag)
    {
        // プレイヤーの入力結果を見る
        var result = DetectInputInternal(flag);

        // 入力結果が有効ならターン終了
        if (result == true)
            m_CharaTurn.TurnEnd();
    }

    /// <summary>
    /// 入力検知 攻撃、移動
    /// </summary>
    /// <param name="flag"></param>
    private bool DetectInputInternal(KeyCodeFlag flag)
    {
        // 行動許可ないなら何もしない。行動中なら何もしない。
        if (m_CharaTurn.CanAct == false || m_CharaTurn.IsActing == true)
            return false;

        // Ui操作中なら何もしない
        if (InputManager.Interface.IsUiPopUp == true)
            return false;

        // 攻撃
        if (DetectInputAttack(flag) == true)
            return true;

        // 移動
        if (DetectInputMove(flag) == true)
            return true;

        return false;
    }

    /// <summary>
    /// 攻撃
    /// </summary>
    /// <param name="flag"></param>
    /// <returns></returns>
    private bool DetectInputAttack(KeyCodeFlag flag)
    {
        if (flag.HasBitFlag(KeyCodeFlag.E))
            return m_CharaBattle.NormalAttack();

        return false;
    }

    /// <summary>
    /// 移動
    /// </summary>
    /// <param name="flag"></param>
    /// <returns></returns>
    private bool DetectInputMove(KeyCodeFlag flag)
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

        // 斜め入力限定
        bool diagonal = flag.HasBitFlag(KeyCodeFlag.Right_Shift);
        if (diagonal == true && JudgeDirectionDiagonal(direction) == false)
            return false;

        return m_CharaMove.Move(direction.ToDirEnum());
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