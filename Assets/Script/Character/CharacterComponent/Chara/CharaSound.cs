using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public interface ICharaSound : IActorInterface
{

}

public class CharaSound : ActorComponentBase, ICharaSound
{
    private static bool ms_IsSet = false;

    /// <summary>
    /// 攻撃
    /// </summary>
    private static AudioSource ms_AttackSound;

    /// <summary>
    /// 空振り
    /// </summary>
    private static AudioSource ms_MissSound;

    /// <summary>
    /// 被ダメージ
    /// </summary>
    private static AudioSource ms_DamageSound;

    protected override void Register(ICollector owner)
    {
        base.Register(owner);
        owner.Register<ICharaSound>(this);
    }

    protected override void Initialize()
    {
        base.Initialize();

        if (ms_IsSet == false)
        {
            ms_IsSet = true;
            var attack = Instantiate(MasterDataHolder.Interface.CharacterMasterSetup.AttackSound);
            ms_AttackSound = attack.GetComponent<AudioSource>();
            ms_MissSound = Instantiate(MasterDataHolder.Interface.CharacterMasterSetup.MissSound).GetComponent<AudioSource>();
            ms_DamageSound = Instantiate(MasterDataHolder.Interface.CharacterMasterSetup.DamageSound).GetComponent<AudioSource>();
        }

        if (Owner.RequireEvent<ICharaBattleEvent>(out var battle) == true)
        {
            battle.OnAttackStart.Subscribe(_ =>
            {
                // 攻撃音
                StartCoroutine(Coroutine.DelayCoroutine(CharaBattle.ms_NormalAttackHitTime, () =>
                {
                    ms_AttackSound.Play();
                }));
            }).AddTo(CompositeDisposable);

            battle.OnAttackEnd.Subscribe(result =>
            {
                if (result.IsHit == false)
                {
                    // 空振り音
                    ms_MissSound.Play();
                }
            }).AddTo(CompositeDisposable);

            battle.OnDamageEnd.Subscribe(_ =>
            {
                // ダメージ音
                ms_DamageSound.Play();
            }).AddTo(CompositeDisposable);
        }

    }
}