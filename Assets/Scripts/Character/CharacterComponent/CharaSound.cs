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
            battle.OnAttackStart.SubscribeWithState(this, (_, self) =>
            {
                // 攻撃音
                self.StartCoroutine(Coroutine.DelayCoroutine(CharaBattle.ms_NormalAttackHitTime, this, self =>
                {
                    self.m_SoundHolder.AttackSound.Play();
                }));
            }).AddTo(CompositeDisposable);

            battle.OnAttackEnd.SubscribeWithState(this, (result, self) =>
            {
                if (result.IsHit == false)
                {
                    // 空振り音
                    self.m_SoundHolder.MissSound.Play();
                }
            }).AddTo(CompositeDisposable);

            battle.OnDamageEnd.SubscribeWithState(this, (_, self) =>
            {
                // ダメージ音
                self.m_SoundHolder.DamageSound.Play();
            }).AddTo(CompositeDisposable);
        }

    }
}