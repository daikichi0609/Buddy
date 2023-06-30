using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using Zenject;
using System.Threading.Tasks;

public interface ICharaSound : IActorInterface
{

}

public class CharaSound : ActorComponentBase, ICharaSound
{
    [Inject]
    private ISoundHolder m_SoundHolder;

    private static readonly string ATTACK_SOUND = "Attack";
    private static readonly string DAMAGE_SOUND = "Damage";
    private static readonly string MISS_SOUND = "Miss";

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
            // 攻撃音
            battle.OnAttackStart.SubscribeWithState(this, async (_, self) =>
            {
                await Task.Delay((int)(CharaBattle.ms_NormalAttackHitTime * 1000));
                if (self.m_SoundHolder.TryGetSound(ATTACK_SOUND, out var sound) == true)
                    sound.Play();
            }).AddTo(Owner.Disposables);

            // 空振り音
            battle.OnAttackEnd.SubscribeWithState(this, (result, self) =>
            {
                if (result.IsHit == false)
                    if (self.m_SoundHolder.TryGetSound(MISS_SOUND, out var sound) == true)
                        sound.Play();
            }).AddTo(Owner.Disposables);

            // ダメージ音
            battle.OnDamageEnd.SubscribeWithState(this, (_, self) =>
            {
                if (self.m_SoundHolder.TryGetSound(DAMAGE_SOUND, out var sound) == true)
                    sound.Play();
            }).AddTo(Owner.Disposables);
        }

    }
}