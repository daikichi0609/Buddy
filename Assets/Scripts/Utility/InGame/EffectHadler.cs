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
    /// <param name="gameObject"></param>
    void RegisterEffect(GameObject gameObject, GameObject sound);

    /// <summary>
    /// エフェクトセット
    /// </summary>
    /// <param name="holder"></param>
    void RegisterEffect(ParticleSystemHolder holder, GameObject sound);

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
    private ParticleSystemHolder m_ParticleSystemHolder;

    /// <summary>
    /// サウンド
    /// </summary>
    private AudioSource m_AudioSource;

    /// <summary>
    /// エフェクトセット
    /// </summary>
    /// <param name="holder"></param>
    /// <param name="pos"></param>
    private void RegisterEffect(ParticleSystemHolder holder, GameObject sound)
    {
#if DEBUG
        if (m_ParticleSystemHolder != null)
            Debug.LogWarning("既にParticleSystemが登録済みです。上書きします。");
        if (m_AudioSource != null)
            Debug.LogWarning("既にAudioSourceが登録済みです。上書きします。");
#endif

        m_ParticleSystemHolder = holder;
        m_AudioSource = sound.GetComponent<AudioSource>();
    }
    void IEffectHandler.RegisterEffect(ParticleSystemHolder holder, GameObject sound) => RegisterEffect(holder, sound);
    void IEffectHandler.RegisterEffect(GameObject gameObject, GameObject sound) => RegisterEffect(gameObject.GetComponent<ParticleSystemHolder>(), sound);

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
        var stop = m_ParticleSystemHolder.Play(pos);
        await Task.Delay((int)(time * 1000));
        stop.Dispose();
    }

    void IDisposable.Dispose()
    {
        MonoBehaviour.Destroy(m_ParticleSystemHolder.gameObject);
        MonoBehaviour.Destroy(m_AudioSource.gameObject);
    }
}
