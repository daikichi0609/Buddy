using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public interface ICharaSound : ICharacterInterface
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

        if (Owner.RequireEvent<ICharaBattleEvent>(out var battle) == true)
        {
            battle.OnAttackStart.Subscribe(_ =>
            {
                // 攻撃音
                StartCoroutine(Coroutine.DelayCoroutine(CharaBattle.ms_NormalAttackHitTime, () =>
                {
                    SoundManager.Instance.Attack_Sword.Play();
                }));
            }).AddTo(Disposable);

            battle.OnAttackEnd.Subscribe(result =>
            {
                if (result.IsHit == false)
                {
                    // 空振り音
                    SoundManager.Instance.Miss.Play();
                }
            }).AddTo(Disposable);

            battle.OnDamageEnd.Subscribe(_ =>
            {
                // ダメージ音
                SoundManager.Instance.Damage_Small.Play();
            }).AddTo(Disposable);
        }

    }
}