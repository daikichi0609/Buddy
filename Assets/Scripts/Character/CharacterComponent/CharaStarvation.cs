using UnityEngine;
using UniRx;
using System;
using System.Collections.Generic;
using Zenject;

public interface ICharaStarvation : IActorInterface
{
    /// <summary>
    /// 空腹値
    /// </summary>
    int Hungry { get; set; }

    /// <summary>
    /// 空腹状態かどうか
    /// </summary>
    bool IsStarvate { get; }

    /// <summary>
    /// 空腹値減少
    /// </summary>
    /// <param name="add"></param>
    void RecoverHungry(int add);
}

/// <summary>
/// 自動回復
/// </summary>
public class CharaStarvation : ActorComponentBase, ICharaStarvation
{
    [Inject]
    private ITurnManager m_TurnManager;
    [Inject]
    private IBattleLogManager m_BattleLogManager;
    [Inject]
    private ISoundHolder m_SoundHolder;

    /// <summary>
    /// 空腹値
    /// </summary>
    private int m_Hungry = 0;
    int ICharaStarvation.Hungry { get => m_Hungry; set => m_Hungry = value; }

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
    private int HUNGRY_08 => (int)(MAX_HUNGRY * 0.8f);
    /// <summary>
    /// 空腹値
    /// </summary>
    private int HUNGRY_09 => (int)(MAX_HUNGRY * 0.9f);

    private static readonly string MESSAGE_08 = "おなかが減ってきた…。";
    private static readonly string MESSAGE_09 = "空腹で目がまわってきた…。";
    private int StarvateIndex { get; set; } = 0;
    private static readonly string[] STARVATE_MESSAGE = new string[3]
    {
        "ダメだ！もうげんかいだ…。",
        "はやく、なにかたべないと！",
        "たおれてしまう！"
    };
    private static readonly string HUNGRY = "Hungry";

    /// <summary>
    /// 空腹値減少インターバル
    /// </summary>
    private static readonly int HUNGRY_TURN = 3;

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
            turn.OnTurnEnd.SubscribeWithState(this, (_, self) =>
            {
                if (self.IsStarvate == true)
                    self.Starvate();
                else
                {
                    self.StarvateIndex = 0;
                    self.MakeHungry();
                }
            }).AddTo(Owner.Disposables);
        }
    }

    /// <summary>
    /// 空腹を回復する
    /// </summary>
    /// <param name="add"></param>
    void ICharaStarvation.RecoverHungry(int add) => m_Hungry = Mathf.Clamp(m_Hungry - add, 0, MAX_HUNGRY);

    /// <summary>
    /// 空腹状態
    /// </summary>
    private void Starvate()
    {
        int currentTurn = m_TurnManager.TotalTurnCount + 1;

        // ダメージインターバル
        if (currentTurn % STARVATION_TURN != 0)
            return;

        if (Owner.RequireInterface<ICharaStatus>(out var status) == false)
            return;

        // 死亡はしない
        status.CurrentStatus.RecoverHp(-1, false);

        if (StarvateIndex < STARVATE_MESSAGE.Length)
        {
            if (StarvateIndex == 0)
                if (m_SoundHolder.TryGetSound(HUNGRY, out var sound) == true)
                    sound.Play();

            m_BattleLogManager.Log(STARVATE_MESSAGE[StarvateIndex]);
            StarvateIndex++;
        }
    }

    /// <summary>
    /// 空腹ゲージ増加
    /// </summary>
    private void MakeHungry()
    {
        int currentTurn = m_TurnManager.TotalTurnCount + 1;

        // 空腹インターバル
        if (currentTurn % HUNGRY_TURN != 0)
            return;

        m_Hungry = Mathf.Clamp(++m_Hungry, 0, MAX_HUNGRY);

        // 8割メッセ
        if (m_Hungry == HUNGRY_08)
        {
            if (m_SoundHolder.TryGetSound(HUNGRY, out var sound) == true)
                sound.Play();
            m_BattleLogManager.Log(MESSAGE_08);
        }

        // 9割メッセ
        if (m_Hungry == HUNGRY_09)
        {
            if (m_SoundHolder.TryGetSound(HUNGRY, out var sound) == true)
                sound.Play();
            m_BattleLogManager.Log(MESSAGE_09);
        }
    }
}