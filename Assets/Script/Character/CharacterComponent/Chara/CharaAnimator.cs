using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using System.Threading.Tasks;
using NaughtyAttributes;

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

public interface ICharaAnimator : ICharacterComponent
{

}

public class CharaAnimator : CharaComponentBase, ICharaAnimator
{
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

    protected override void Register(ICollector owner)
    {
        base.Register(owner);
        owner.Register<ICharaAnimator>(this);
    }

    protected override void Initialize()
    {
        base.Initialize();
        m_CharaTurn = Owner.GetComponent<ICharaTurn>();

        // 特定のアニメーション中は他キャラの行動を禁止する
        AnimationStateChanged
            .Zip(AnimationStateChanged.Skip(1), (Old, New) => new { Old, New })
            .Subscribe(state =>
            {
                switch (state.New)
                {
                    case ANIMATION_TYPE.ATTACK:
                    case ANIMATION_TYPE.DAMAGE:
                        TurnManager.Interface.ProhibitAllAction = true;
                        return;
                }

                switch (state.Old)
                {
                    case ANIMATION_TYPE.ATTACK:
                    case ANIMATION_TYPE.DAMAGE:
                        TurnManager.Interface.ProhibitAllAction = false;
                        return;
                }
            }).AddTo(this);

        if (Owner.RequireComponent<ICharaBattleEvent>(out var battle) == true)
        {
            // 攻撃
            battle.OnAttackStart.Subscribe(async _ => await PlayAnimation(ANIMATION_TYPE.ATTACK, CharaBattle.ms_NormalAttackTotalTime)).AddTo(this);

            // ダメージ前
            battle.OnDamageStart.Subscribe(async _ => await PlayAnimation(ANIMATION_TYPE.DAMAGE, CharaBattle.ms_DamageTotalTime)).AddTo(this);
        }

        if (Owner.RequireComponent<ICharaMoveEvent>(out var move) == true)
        {
            // 移動前
            move.OnMoveStart.Subscribe(_ => PlayAnimation(ANIMATION_TYPE.MOVE)).AddTo(this);

            // 移動後
            move.OnMoveEnd.Subscribe(_ => StopAnimation(ANIMATION_TYPE.MOVE)).AddTo(this);
        }
    }

    /// <summary>
    /// モーション流す
    /// </summary>
    /// <param name="type"></param>
    private void PlayAnimation(ANIMATION_TYPE type)
    {
        m_CharaTurn.IsActing = true;
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
        m_CharaTurn.IsActing = false;
        m_AnimationState.Value = ANIMATION_TYPE.IDLE;
        m_CharaAnimator.SetBool(GetKey(type), false);
    }

    /// <summary>
    /// アニメーション切り替えキー取得
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private string GetKey(ANIMATION_TYPE type)
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