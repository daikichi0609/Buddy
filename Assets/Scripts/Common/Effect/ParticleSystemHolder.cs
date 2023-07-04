using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UniRx;
using UnityEngine;

public class ParticleSystemHolder : MonoBehaviour
{
    [SerializeField]
    private ParticleSystem[] m_ParticleSystems;

    [SerializeField, ReadOnly]
    private bool m_IsPlaying;
    public bool IsPlaying => m_IsPlaying;

    public IDisposable Play(Vector3 pos)
    {
        transform.position = pos;
        m_IsPlaying = true;
        foreach (var particle in m_ParticleSystems)
            particle.Play();

        return Disposable.CreateWithState(this, self =>
        {
            self.m_IsPlaying = false;
            foreach (var particle in self.m_ParticleSystems)
                particle.Stop();
        });
    }
}
