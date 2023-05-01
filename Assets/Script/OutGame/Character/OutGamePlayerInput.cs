using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public interface IOutGamePlayerInput : IActorInterface
{
    /// <summary>
    /// 操作可能か
    /// </summary>
    bool CanOperate { get; set; }
}

public class OutGamePlayerInput : ActorComponentBase, IOutGamePlayerInput
{
    /// <summary>
    /// キャラコン
    /// </summary>
    private ICharaController m_CharaController;

    /// <summary>
    /// 操作可能か
    /// </summary>
    private bool m_CanOperate;
    bool IOutGamePlayerInput.CanOperate { get => m_CanOperate; set => m_CanOperate = value; }

    protected override void Register(ICollector owner)
    {
        base.Register(owner);
        owner.Register<IOutGamePlayerInput>(this);
    }

    protected override void Initialize()
    {
        base.Initialize();

        // 入力購読
        InputManager.Interface.InputEvent.Subscribe(input =>
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

        // 移動検知
        DetectInput(flag);
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
        m_CharaController.Move(direction);
        return true;
    }
}
