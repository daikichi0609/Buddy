using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public interface IEffectHandler : IDisposable
{
    /// <summary>
    /// エフェクトセット
    /// </summary>
    /// <param name="effect"></param>
    void RegisterEffect(GameObject effect, GameObject sound);

    /// <summary>
    /// エフェクト再生
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="time"></param>
    /// <returns></returns>
    Task Play(Vector3 pos, float time = 1f);
}

public class EffectHandler : IEffectHandler
{
    /// <summary>
    /// エフェクトプレハブ
    /// </summary>
    private GameObject m_ParentObject;
    private ParticleSystemHolder m_ParticleSystemHolder;

    /// <summary>
    /// サウンド
    /// </summary>
    private AudioSource m_AudioSource;

    /// <summary>
    /// エフェクトセット
    /// </summary>
    /// <param name="effect"></param>
    /// <param name="pos"></param>
    void IEffectHandler.RegisterEffect(GameObject effect, GameObject sound)
    {
#if DEBUG
        if (m_ParentObject != null)
            Debug.Log("既にParticleSystemが登録済みです。上書きします。");
#endif

        m_ParentObject = effect;
        m_ParticleSystemHolder = m_ParentObject.GetComponent<ParticleSystemHolder>();
        m_AudioSource = sound.GetComponent<AudioSource>();
    }

    /// <summary>
    /// エフェクト再生
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="time"></param>
    /// <returns></returns>
    async Task IEffectHandler.Play(Vector3 pos, float time)
    {
        m_AudioSource.Play();
        await PlayInternal(pos, time);
    }

    private async Task PlayInternal(Vector3 pos, float time)
    {
        m_ParentObject.transform.position = pos;

        foreach (var effect in m_ParticleSystemHolder.ParticleSystems)
            effect.Play();

        await Task.Delay((int)(time * 1000));

        foreach (var effect in m_ParticleSystemHolder.ParticleSystems)
            effect.Stop();
    }

    void IDisposable.Dispose()
    {
        MonoBehaviour.Destroy(m_ParentObject);
        m_ParticleSystemHolder = null;
        MonoBehaviour.Destroy(m_AudioSource.gameObject);
    }
}
