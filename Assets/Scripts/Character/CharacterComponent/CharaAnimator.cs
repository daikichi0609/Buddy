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

    /// <summary>
    /// 眠り
    /// </summary>
    SLEEP,
}

public interface ICharaAnimator : IActorInterface
{
    IDisposable PlayAnimation(ANIMATION_TYPE type);
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
    /// IsActingフラグ解除
    /// </summary>
    private IDisposable m_CancelAct;

    protected override void Register(ICollector owner)
    {
        base.Register(owner);
        owner.Register<ICharaAnimator>(this);
    }

    protected override void Initialize()
    {
        base.Initialize();
        m_CharaTurn = Owner.GetInterface<ICharaTurn>();

        if (Owner.RequireEvent<ICharaBattleEvent>(out var battle) == true)
        {
            // 攻撃
            battle.OnAttackStart.SubscribeWithState(this, async (_, self) => await self.PlayAnimation(ANIMATION_TYPE.ATTACK, CharaBattle.ms_NormalAttackTotalTime)).AddTo(Owner.Disposables);

            // ダメージ前
            battle.OnDamageStart.SubscribeWithState(this, async (result, self) =>
            {
                if (result.IsHit == true)
                    await self.PlayAnimation(ANIMATION_TYPE.DAMAGE, CharaBattle.ms_DamageTotalTime);
            }).AddTo(Owner.Disposables);
        }

        if (Owner.RequireEvent<ICharaMoveEvent>(out var move) == true)
        {
            // 移動前
            move.OnMoveStart.SubscribeWithState(this, (_, self) => self.PlayAnimation(ANIMATION_TYPE.MOVE)).AddTo(Owner.Disposables);

            // 移動後
            move.OnMoveEnd.SubscribeWithState(this, (_, self) => self.StopAnimation(ANIMATION_TYPE.MOVE)).AddTo(Owner.Disposables);
        }
    }

    /// <summary>
    /// モーション流す
    /// </summary>
    /// <param name="type"></param>
    private void PlayAnimation(ANIMATION_TYPE type)
    {
        if (m_CancelAct != null)
        {
#if DEBUG
            Debug.LogWarning("再生途中のアニメーションをキャンセルします。");
            m_CancelAct.Dispose();
            m_CancelAct = null;
#endif
        }

        m_CancelAct = m_CharaTurn.RegisterActing();
        m_AnimationState.Value = type;
        m_CharaAnimator.SetBool(GetKey(type), true);
    }

    /// <summary>
    /// 時間指定でモーション流す
    /// </summary>
    /// <param name="type"></param>
    /// <param name="time"></param>
    private async Task PlayAnimation(ANIMATION_TYPE type, float time)
    {
        PlayAnimation(type);
        await Task.Delay((int)(time * 1000));
        StopAnimation(type);
    }

    IDisposable ICharaAnimator.PlayAnimation(ANIMATION_TYPE type)
    {
        m_CharaAnimator.SetBool(GetKey(type), true);
        return Disposable.CreateWithState(this, self => m_CharaAnimator.SetBool(GetKey(type), false));
    }

    /// <summary>
    /// モーション止める
    /// </summary>
    /// <param name="type"></param>
    private void StopAnimation(ANIMATION_TYPE type)
    {
        if (m_CancelAct != null)
        {
            m_CancelAct.Dispose();
            m_CancelAct = null;
        }

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
            ANIMATION_TYPE.SLEEP => "IsSleeping",
            _ => "",
        };

        return key;
    }
}