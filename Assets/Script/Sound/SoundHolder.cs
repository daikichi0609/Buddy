using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public interface ISoundHolder : ISingleton
{
    AudioSource AttackSound { get; }
    AudioSource MissSound { get; }
    AudioSource DamageSound { get; }
}

public class SoundHolder : Singleton<SoundHolder, ISoundHolder>, ISoundHolder
{
    protected override void Awake()
    {
        base.Awake();
        PlayerLoopManager.Interface.GetInitEvent.Subscribe(_ => Initialize()).AddTo(this);
    }

    /// <summary>
    /// インスタンス生成
    /// </summary>
    private void Initialize()
    {
        m_AttackSound = Instantiate(MasterDataHolder.Interface.CharacterMasterSetup.AttackSound).GetComponent<AudioSource>();
        m_MissSound = Instantiate(MasterDataHolder.Interface.CharacterMasterSetup.MissSound).GetComponent<AudioSource>();
        m_DamageSound = Instantiate(MasterDataHolder.Interface.CharacterMasterSetup.DamageSound).GetComponent<AudioSource>();
    }

    /// <summary>
    /// 攻撃
    /// </summary>
    private AudioSource m_AttackSound;
    AudioSource ISoundHolder.AttackSound => m_AttackSound;

    /// <summary>
    /// 空振り
    /// </summary>
    private AudioSource m_MissSound;
    AudioSource ISoundHolder.MissSound => m_MissSound;

    /// <summary>
    /// 被ダメージ
    /// </summary>
    private AudioSource m_DamageSound;
    AudioSource ISoundHolder.DamageSound => m_DamageSound;
}
