using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemHolder : MonoBehaviour
{
    [SerializeField]
    private ParticleSystem[] m_ParticleSystems;
    public ParticleSystem[] ParticleSystems => m_ParticleSystems;
}
