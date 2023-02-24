using UnityEngine;
using UniRx;
using System;
using System.Collections.Generic;

public interface ICharaStarvation : ICharacterComponent
{
    bool IsStarvate { get; }
}

/// <summary>
/// 自動回復
/// </summary>
public class CharaStarvation : CharaComponentBase, ICharaStarvation
{
    /// <summary>
    /// 空腹値
    /// </summary>
    private int m_Hungry = 0;

    /// <summary>
    /// 空腹であるかどうか
    /// </summary>
    private bool IsStarvate => m_Hungry >= MAX_HUNGRY;
    bool ICharaStarvation.IsStarvate => IsStarvate;

    /// <summary>
    /// 空腹値最大
    /// </summary>
    private static readonly int MAX_HUNGRY = 100;

    private static readonly int HUNGRY_TURN = 10;
    private static readonly int STARVATION_TURN = 1;

    protected override void Register(ICollector owner)
    {
        base.Register(owner);
        owner.Register<ICharaStarvation>(this);
    }

    protected override void Initialize()
    {
        base.Initialize();

        if (Owner.RequireComponent<ICharaTurnEvent>(out var turn) == true)
        {
            turn.OnTurnEnd.Subscribe(_ =>
            {
                if (IsStarvate == true)
                    Starvate();
                else
                    MakeHungry();
            }).AddTo(this);
        }
    }

    /// <summary>
    /// 空腹状態
    /// </summary>
    private void Starvate()
    {
        int currentTurn = TurnManager.Interface.TotalTurnCount + 1;

        // ダメージインターバル
        if (currentTurn % STARVATION_TURN == 0)
            return;

        if (Owner.RequireComponent<ICharaStatus>(out var status) == false)
            return;

        m_Hungry++;
    }

    /// <summary>
    /// 空腹ゲージ増加
    /// </summary>
    private void MakeHungry()
    {
        int currentTurn = TurnManager.Interface.TotalTurnCount + 1;

        // 空腹インターバル
        if (currentTurn % HUNGRY_TURN == 0)
            return;

        if (Owner.RequireComponent<ICharaStatus>(out var status) == false)
            return;

        Mathf.Clamp(++m_Hungry, 0, MAX_HUNGRY);
    }
}