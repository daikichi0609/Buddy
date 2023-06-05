using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class HomeSystemInstaller : MonoInstaller
{
    [SerializeField]
    private GameObject m_HomeSystem;

    public override void InstallBindings()
    {
        // 初期化
        Container.Bind<ISceneInitializer>()
            .To<HomeInitializer>()
            .FromComponentOn(m_HomeSystem)
            .AsSingle()
            .NonLazy();
    }
}