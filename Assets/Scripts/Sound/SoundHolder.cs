using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using Zenject;

public interface ISoundHolder
{
    AudioSource AttackSound { get; }
    AudioSource MissSound { get; }
    AudioSource DamageSound { get; }
}

public class SoundHolder : ISoundHolder
{
    [Inject]
    private CharacterMasterSetup m_CharacterMasterSetup;

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
        loopManager.GetInitEvent.SubscribeWithState(this, (_, self) => self.Initialize());
    }

    /// <summary>
    /// インスタンス生成
    /// </summary>
    private void Initialize()
    {
        m_AttackSound = MonoBehaviour.Instantiate(m_CharacterMasterSetup.AttackSound).GetComponent<AudioSource>();
        m_MissSound = MonoBehaviour.Instantiate(m_CharacterMasterSetup.MissSound).GetComponent<AudioSource>();
        m_DamageSound = MonoBehaviour.Instantiate(m_CharacterMasterSetup.DamageSound).GetComponent<AudioSource>();
    }
}
