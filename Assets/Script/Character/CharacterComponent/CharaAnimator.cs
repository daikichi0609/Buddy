using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

/// <summary>
/// アニメーションパターン定義
/// </summary>
public enum ANIMATION_TYPE
{
    IDLE,
    MOVE,
    ATTACK,
    DAMAGE,
}

public interface ICharaAnimator : ICharacterComponent
{
    void PlayAnimation(ANIMATION_TYPE type);

    void StopAnimation(ANIMATION_TYPE type);

    bool IsCurrentState(string state);
}

public class CharaAnimator : CharaComponentBase, ICharaAnimator
{
    /// <summary>
    /// キャラが持つアニメーター
    /// </summary>
    [SerializeField]
    private Animator m_CharaAnimator;

    void ICharaAnimator.PlayAnimation(ANIMATION_TYPE type) => m_CharaAnimator.SetBool(GetKey(type), true);

    void ICharaAnimator.StopAnimation(ANIMATION_TYPE type) => m_CharaAnimator.SetBool(GetKey(type), false);

    bool ICharaAnimator.IsCurrentState(string state) => m_CharaAnimator.GetCurrentAnimatorStateInfo(0).IsName(state);

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