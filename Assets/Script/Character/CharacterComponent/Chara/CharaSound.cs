using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public interface ICharaSound : IActorInterface
{

}

public class CharaSound : ActorComponentBase, ICharaSound
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
                    SoundHolder.Interface.AttackSound.Play();
                }));
            }).AddTo(CompositeDisposable);

            battle.OnAttackEnd.Subscribe(result =>
            {
                if (result.IsHit == false)
                {
                    // 空振り音
                    SoundHolder.Interface.MissSound.Play();
                }
            }).AddTo(CompositeDisposable);

            battle.OnDamageEnd.Subscribe(_ =>
            {
                // ダメージ音
                SoundHolder.Interface.DamageSound.Play();
            }).AddTo(CompositeDisposable);
        }

    }
}