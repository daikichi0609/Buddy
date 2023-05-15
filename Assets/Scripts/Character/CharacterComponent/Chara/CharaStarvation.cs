using UnityEngine;
using UniRx;
using System;
using System.Collections.Generic;

public interface ICharaStarvation : IActorInterface
{
    bool IsStarvate { get; }
    void RecoverHungry(int add);
}

/// <summary>
/// 自動回復
/// </summary>
public class CharaStarvation : ActorComponentBase, ICharaStarvation
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
    /// <summary>
    /// 空腹値半分
    /// </summary>
    private int HALF => (int)(MAX_HUNGRY * 0.5f);
    /// <summary>
    /// 空腹値
    /// </summary>
    private int QUARTER => (int)(MAX_HUNGRY * 0.25f);

    private static readonly string HALF_MESSAGE = "おなかが減ってきた…。";
    private static readonly string QUARTER_MESSAGE = "おなかがグルグルと鳴っている…。";
    private int StarvateIndex { get; set; } = 0;
    private static readonly string[] STARVATE_MESSAGE = new string[3]
    {
        "ダメだ！もうげんかいだ…。",
        "はやく、なにかたべないと！",
        "たおれてしまう！"
    };

    /// <summary>
    /// 空腹値減少インターバル
    /// </summary>
    private static readonly int HUNGRY_TURN = 10;

    /// <summary>
    /// 空腹による体力減少インターバル
    /// </summary>
    private static readonly int STARVATION_TURN = 1;

    protected override void Register(ICollector owner)
    {
        base.Register(owner);
        owner.Register<ICharaStarvation>(this);
    }

    protected override void Initialize()
    {
        base.Initialize();

        if (Owner.RequireEvent<ICharaTurnEvent>(out var turn) == true)
        {
            turn.OnTurnEndPost.Subscribe(_ =>
            {
                if (IsStarvate == true)
                    Starvate();
                else
                    MakeHungry();
            }).AddTo(CompositeDisposable);
        }
    }

    /// <summary>
    /// 空腹を回復する
    /// </summary>
    /// <param name="add"></param>
    void ICharaStarvation.RecoverHungry(int add)
    {
        m_Hungry += add;
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

        if (Owner.RequireInterface<ICharaStatus>(out var status) == false)
            return;

        // 死亡はしない
        Mathf.Clamp(--status.CurrentStatus.Hp, 1, status.Parameter.MaxHp);
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

        if (Owner.RequireInterface<ICharaStatus>(out var status) == false)
            return;

        Mathf.Clamp(++m_Hungry, 0, MAX_HUNGRY);

        if (m_Hungry == HALF)
            BattleLogManager.Interface.Log(HALF_MESSAGE);

        if (m_Hungry == QUARTER)
            BattleLogManager.Interface.Log(QUARTER_MESSAGE);

        if (m_Hungry == MAX_HUNGRY)
        {
            if (StarvateIndex >= STARVATE_MESSAGE.Length)
                return;

            BattleLogManager.Interface.Log(STARVATE_MESSAGE[StarvateIndex]);
            StarvateIndex++;
        }
        else
            StarvateIndex = 0;
    }
}