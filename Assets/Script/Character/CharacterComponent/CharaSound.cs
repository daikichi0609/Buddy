using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public interface ICharaSound : ICharacterComponent
{

}

public class CharaSound : CharaComponentBase, ICharaSound
{
    protected override void Register(ICollector owner)
    {
        base.Register(owner);
        owner.Register<ICharaSound>(this);
    }

    protected override void Initialize()
    {
        base.Initialize();

        if (Owner.RequireComponent<ICharaBattleEvent>(out var battle) == true)
        {
            battle.OnAttackStart.Subscribe(_ =>
            {
                // 攻撃音
                StartCoroutine(Coroutine.DelayCoroutine(CharaBattle.ms_NormalAttackHitTime, () =>
                {
                    SoundManager.Instance.Attack_Sword.Play();
                }));
            }).AddTo(this);

            battle.OnAttackEnd.Subscribe(result =>
            {
                if(result.IsHit == false)
                {
                    // 空振り音
                    SoundManager.Instance.Miss.Play();
                }
            }).AddTo(this);

            battle.OnDamageEnd.Subscribe(_ =>
            {
                // ダメージ音
                SoundManager.Instance.Damage_Small.Play();
            }).AddTo(this);
        }

    }
}