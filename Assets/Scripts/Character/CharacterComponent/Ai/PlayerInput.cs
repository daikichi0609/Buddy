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
}

/// <summary>
/// 入力機能
/// </summary>
public class PlayerInput : ActorComponentBase, IPlayerInput
{
    [Inject]
    private IInputManager m_InputManager;
    [Inject]
    private ICameraHandler m_CameraHandler;
    [Inject]
    private IDungeonHandler m_DungeonHandler;

    private ICharaBattle m_CharaBattle;
    private ICharaMove m_CharaMove;
    private ICharaTurn m_CharaTurn;

    // カメラ追従キャンセル
    private IDisposable m_CancelFollowing;

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

        // 入力購読
        m_InputManager.InputEvent.SubscribeWithState(this, (input, self) =>
        {
            self.DetectInput(input.KeyCodeFlag);
        }).AddTo(CompositeDisposable);


        // ----- 別のとこに移した方が良い ----- //
        // カメラ追従
        if (Owner.RequireInterface<ICharaObjectHolder>(out var holder) == true)
        {
            m_CancelFollowing = m_CameraHandler.SetParent(holder.MoveObject); // カメラをリーダーに追従させる

            // 死亡時
            var battle = Owner.GetEvent<ICharaBattleEvent>();
            battle.OnDead.SubscribeWithState(this, (_, self) => self.m_CancelFollowing.Dispose()).AddTo(CompositeDisposable);

            // その他
            CompositeDisposable.Add(m_CancelFollowing);
        }

        // 探索済みとしてマーク
        if (Owner.RequireEvent<ICharaTurnEvent>(out var e) == true)
        {
            e.OnTurnEndPost.SubscribeWithState(this, (_, self) =>
            {
                var pos = self.m_CharaMove.Position;
                self.m_DungeonHandler.MarkExplored(pos);
            }).AddTo(CompositeDisposable);
        }
        // ----- //
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
        if (m_InputManager.IsUiPopUp == true)
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

        // 方向転換だけ
        bool onlyFace = flag.HasBitFlag(KeyCodeFlag.Right_Shift);
        if (onlyFace == true)
        {
            m_CharaMove.Face(direction.ToDirEnum());
            return false; // ターン消費しない
        }

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