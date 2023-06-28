using UnityEngine;
using UniRx;
using System;
using System.Threading.Tasks;

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
    /// <summary>
    /// アニメーション流す
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    IDisposable PlayAnimation(ANIMATION_TYPE type);

    /// <summary>
    /// アニメーション流す 任意の時間だけIsActingを立てる
    /// </summary>
    /// <param name="type"></param>
    /// <param name="time"></param>
    /// <returns></returns>
    Task PlayAnimation(ANIMATION_TYPE type, float time);
}

public class CharaAnimator : ActorComponentBase, ICharaAnimator
{
    private ICharaTurn m_CharaTurn;

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
        Owner.RequireInterface(out m_CharaTurn);
    }

    private IDisposable PlayAnimation(ANIMATION_TYPE type)
    {
        var key = GetKey(type);
        m_CharaAnimator.SetBool(key, true);
        return Disposable.CreateWithState((this, key), tuple => tuple.Item1.m_CharaAnimator.SetBool(tuple.key, false));
    }
    IDisposable ICharaAnimator.PlayAnimation(ANIMATION_TYPE type) => PlayAnimation(type);

    async Task ICharaAnimator.PlayAnimation(ANIMATION_TYPE type, float time)
    {
        var acting = m_CharaTurn.RegisterActing();
        var animation = PlayAnimation(type);

        await Task.Delay((int)(time * 1000));

        acting.Dispose();
        animation.Dispose();
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