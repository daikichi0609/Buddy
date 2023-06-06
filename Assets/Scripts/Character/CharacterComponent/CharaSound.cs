using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using Zenject;

public interface ICharaSound : IActorInterface
{

}

public class CharaSound : ActorComponentBase, ICharaSound
{
    [Inject]
    private ISoundHolder m_SoundHolder;

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
                    m_SoundHolder.AttackSound.Play();
                }));
            }).AddTo(CompositeDisposable);

            battle.OnAttackEnd.Subscribe(result =>
            {
                if (result.IsHit == false)
                {
                    // 空振り音
                    m_SoundHolder.MissSound.Play();
                }
            }).AddTo(CompositeDisposable);

            battle.OnDamageEnd.Subscribe(_ =>
            {
                // ダメージ音
                m_SoundHolder.DamageSound.Play();
            }).AddTo(CompositeDisposable);
        }

    }
}