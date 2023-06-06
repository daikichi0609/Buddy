using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public interface IEffectHandler : IDisposable
{
    Task Play(Vector3 pos, float time = 0.5f);
}

public class EffectHandler : IEffectHandler
{
    public EffectHandler(GameObject effect) => SetEffect(effect);

    /// <summary>
    /// エフェクトプレハブ
    /// </summary>
    private GameObject m_Effect;

    /// <summary>
    /// エフェクトセット
    /// </summary>
    /// <param name="effect"></param>
    /// <param name="pos"></param>
    public void SetEffect(GameObject effect)
    {
        m_Effect = effect;
        m_Effect.SetActive(false);
    }

    async Task IEffectHandler.Play(Vector3 pos, float time)
    {
        await PlayInternal(pos, time);
    }

    private async Task PlayInternal(Vector3 pos, float time)
    {
        m_Effect.transform.position = pos;
        m_Effect.SetActive(true);

        await Task.Delay((int)time * 1000);

        m_Effect.SetActive(false);
    }

    void IDisposable.Dispose() => MonoBehaviour.Destroy(m_Effect);
}
