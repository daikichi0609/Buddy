using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using Zenject;

public interface ISoundHolder : ISingleton
{
    AudioSource AttackSound { get; }
    AudioSource MissSound { get; }
    AudioSource DamageSound { get; }
}

public class SoundHolder : ISoundHolder
{
    [Inject]
    private MasterDataHolder m_MasterDataHolder;

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

    [Inject]
    public void Construct(IPlayerLoopManager loopManager)
    {
        loopManager.GetInitEvent.Subscribe(_ => Initialize());
    }

    /// <summary>
    /// インスタンス生成
    /// </summary>
    private void Initialize()
    {
        m_AttackSound = MonoBehaviour.Instantiate(m_MasterDataHolder.CharacterMasterSetup.AttackSound).GetComponent<AudioSource>();
        m_MissSound = MonoBehaviour.Instantiate(m_MasterDataHolder.CharacterMasterSetup.MissSound).GetComponent<AudioSource>();
        m_DamageSound = MonoBehaviour.Instantiate(m_MasterDataHolder.CharacterMasterSetup.DamageSound).GetComponent<AudioSource>();
    }
}
