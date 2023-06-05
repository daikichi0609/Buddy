using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using Zenject;

public interface IOutGamePlayerInput : IActorInterface
{
    /// <summary>
    /// 操作可能か
    /// </summary>
    bool CanOperate { set; }
}

public class OutGamePlayerInput : ActorComponentBase, IOutGamePlayerInput
{
    [Inject]
    private IInputManager m_InputManager;

    private ICharaController m_CharaController;
    private ICharaTalk m_CharaTalk;

    /// <summary>
    /// 操作可能か
    /// </summary>
    private bool m_CanOperate;
    bool IOutGamePlayerInput.CanOperate { set => m_CanOperate = value; }

    protected override void Register(ICollector owner)
    {
        base.Register(owner);
        owner.Register<IOutGamePlayerInput>(this);
    }

    protected override void Initialize()
    {
        base.Initialize();

        m_CharaController = Owner.GetInterface<ICharaController>();
        m_CharaTalk = Owner.GetInterface<ICharaTalk>();

        // 入力購読
        m_InputManager.InputEvent.Subscribe(input =>
        {
            DetectInput(input.KeyCodeFlag);
        }).AddTo(this);
    }

    /// <summary>
    /// 入力検知
    /// </summary>
    /// <param name="flag"></param>
    private void DetectInput(KeyCodeFlag flag)
    {
        // 操作禁止ならなにもしない
        if (m_CanOperate == false)
            return;

        // 話す
        if (DetectInputTalk(flag) == true)
            return;

        // 移動検知
        if (DetectInputMove(flag) == true)
            return;
        else
            m_CharaController.StopAnimation(ANIMATION_TYPE.MOVE); // 移動アニメーション終了
    }

    /// <summary>
    /// 会話
    /// </summary>
    /// <param name="flag"></param>
    /// <returns></returns>
    private bool DetectInputTalk(KeyCodeFlag flag)
    {
        if (flag.HasBitFlag(KeyCodeFlag.E))
            return m_CharaTalk.TryTalk();

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

        // 移動
        m_CharaController.Move(direction.ToDirEnum());
        return true;
    }
}
