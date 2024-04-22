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

    public IDisposable Play(Vector3 pos, Vector3 addRot = new Vector3())
    {
        transform.position = pos;
        transform.eulerAngles += addRot;
        m_IsPlaying = true;
        foreach (var particle in m_ParticleSystems)
            particle.Play();

        return Disposable.CreateWithState((this, addRot), tuple =>
        {
            tuple.Item1.transform.eulerAngles -= tuple.addRot;
            tuple.Item1.m_IsPlaying = false;
            foreach (var particle in tuple.Item1.m_ParticleSystems)
                particle.Stop();
        });
    }

    public IDisposable Play(ICollector unit)
    {
        var objectHolder = unit.GetInterface<ICharaObjectHolder>();
        return Play(objectHolder.CharaObject.transform.position, objectHolder.CharaObject.transform.eulerAngles);
    }

    public IDisposable PlayFollow(ICollector unit)
    {
        var objectHolder = unit.GetInterface<ICharaObjectHolder>();
        transform.SetParent(objectHolder.CharaObject.transform);

        var disposable = new CompositeDisposable();
        var stop = Play(objectHolder.CharaObject.transform.position, objectHolder.CharaObject.transform.eulerAngles);
        disposable.Add(stop);
        disposable.Add(Disposable.CreateWithState(this, self => self.gameObject.transform.parent = null));
        return disposable;
    }
}
