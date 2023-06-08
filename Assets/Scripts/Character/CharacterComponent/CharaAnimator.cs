using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using System.Threading.Tasks;
using NaughtyAttributes;
using Zenject;

/// <summary>
/// アニメーションパターン定義
/// </summary>
public enum ANIMATION_TYPE
{
    /// <summary>
    /// 通常
    /// </summary>
    IDLE,

    /// <summary>
    /// 移動
    /// </summary>
    MOVE,

    /// <summary>
    /// 攻撃
    /// </summary>
    ATTACK,

    /// <summary>
    /// 被ダメージ
    /// </summary>
    DAMAGE,
}

public interface ICharaAnimator : IActorInterface
{

}

public class CharaAnimator : ActorComponentBase, ICharaAnimator
{
    [Inject]
    private ITurnManager m_TurnManager;

    private ICharaTurn m_CharaTurn;

    /// <summary>
    /// 現在のステート
    /// </summary>
    [SerializeField, ReadOnly]
    private ReactiveProperty<ANIMATION_TYPE> m_AnimationState = new ReactiveProperty<ANIMATION_TYPE>(ANIMATION_TYPE.IDLE);
    private IObservable<ANIMATION_TYPE> AnimationStateChanged => m_AnimationState;

    /// <summary>
    /// キャラが持つアニメーター
    /// </summary>
    [SerializeField]
    private Animator m_CharaAnimator;

    /// <summary>
    /// 行動禁止全体フラグ解除
    /// </summary>
    private IDisposable m_CancelRequest;

    /// <summary>
    /// IsActingフラグ解除
    /// </summary>
    private (IDisposable, ANIMATION_TYPE)? m_CancelAct;

    protected override void Register(ICollector owner)
    {
        base.Register(owner);
        owner.Register<ICharaAnimator>(this);
    }

    protected override void Initialize()
    {
        base.Initialize();
        m_CharaTurn = Owner.GetInterface<ICharaTurn>();

        // 特定のアニメーション中は他キャラの行動を禁止する
        AnimationStateChanged
            .Zip(AnimationStateChanged.Skip(1), (Old, New) => new { Old, New })
            .SubscribeWithState(this, (state, self) =>
            {
                switch (state.New)
                {
                    case ANIMATION_TYPE.ATTACK:
                    case ANIMATION_TYPE.DAMAGE:
                        self.m_CancelRequest = self.m_TurnManager.RequestProhibitAction(self.Owner);
                        return;
                }

                switch (state.Old)
                {
                    case ANIMATION_TYPE.ATTACK:
                    case ANIMATION_TYPE.DAMAGE:
                        self.m_CancelRequest?.Dispose();
                        return;
                }
            }).AddTo(CompositeDisposable);

        if (Owner.RequireEvent<ICharaBattleEvent>(out var battle) == true)
        {
            // 攻撃
            battle.OnAttackStart.SubscribeWithState(this, async (_, self) => await self.PlayAnimation(ANIMATION_TYPE.ATTACK, CharaBattle.ms_NormalAttackTotalTime)).AddTo(CompositeDisposable);

            // ダメージ前
            battle.OnDamageStart.SubscribeWithState(this, async (_, self) => await self.PlayAnimation(ANIMATION_TYPE.DAMAGE, CharaBattle.ms_DamageTotalTime)).AddTo(CompositeDisposable);
        }

        if (Owner.RequireEvent<ICharaMoveEvent>(out var move) == true)
        {
            // 移動前
            move.OnMoveStart.SubscribeWithState(this, (_, self) => self.PlayAnimation(ANIMATION_TYPE.MOVE)).AddTo(CompositeDisposable);

            // 移動後
            move.OnMoveEnd.SubscribeWithState(this, (_, self) => self.StopAnimation(ANIMATION_TYPE.MOVE)).AddTo(CompositeDisposable);
        }
    }

    protected override void Dispose()
    {
        m_CancelRequest?.Dispose();
        base.Dispose();
    }

    /// <summary>
    /// モーション流す
    /// </summary>
    /// <param name="type"></param>
    private void PlayAnimation(ANIMATION_TYPE type)
    {
        if (m_CancelAct != null)
        {
            Debug.LogAssertion("キャンセルしていないアニメーションがあります。" + "古：" + m_CancelAct.Value.Item2 + "新" + type);
            m_CancelAct.Value.Item1.Dispose();
        }

        m_CancelAct = (m_CharaTurn.RegisterActing(), type);
        m_AnimationState.Value = type;
        m_CharaAnimator.SetBool(GetKey(type), true);
    }

    /// <summary>
    /// 時間指定でモーション流す
    /// </summary>
    /// <param name="type"></param>
    /// <param name="time"></param>
    private async Task PlayAnimation(ANIMATION_TYPE type, int time)
    {
        PlayAnimation(type);
        await Task.Delay(time);
        StopAnimation(type);
    }

    /// <summary>
    /// モーション止める
    /// </summary>
    /// <param name="type"></param>
    private void StopAnimation(ANIMATION_TYPE type)
    {
        if (m_CancelAct == null)
            Debug.LogAssertion("アニメーションキャンセルのIDisposableがありません。");

        m_CancelAct.Value.Item1.Dispose();
        m_CancelAct = null;
        m_AnimationState.Value = ANIMATION_TYPE.IDLE;
        m_CharaAnimator.SetBool(GetKey(type), false);
    }

    /// <summary>
    /// アニメーション切り替えキー取得
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string GetKey(ANIMATION_TYPE type)
    {
        string key = type switch
        {
            ANIMATION_TYPE.IDLE => "",
            ANIMATION_TYPE.MOVE => "IsRunning",
            ANIMATION_TYPE.ATTACK => "IsAttacking",
            ANIMATION_TYPE.DAMAGE => "IsDamaging",
            _ => "",
        };

        return key;
    }
}