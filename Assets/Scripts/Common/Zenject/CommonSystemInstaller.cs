using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class CommonSystemInstaller : MonoInstaller
{
    [SerializeField]
    private GameObject m_CommonSystem;
    [SerializeField]
    private GameObject m_CommonUiSystem;

    public override void InstallBindings()
    {
        // プレハブ生成
        Container.Bind<IInstantiater>()
            .To<Instantiater>()
            .FromComponentOn(m_CommonSystem)
            .AsSingle()
            .NonLazy();

        // PlayerLoop
        Container.Bind<IPlayerLoopManager>()
            .To<PlayerLoopManager>()
            .FromComponentOn(m_CommonSystem)
            .AsSingle()
            .NonLazy();

        // 入力
        Container.Bind<IInputManager>()
            .To<InputManager>()
            .FromComponentOn(m_CommonSystem)
            .AsSingle()
            .NonLazy();

        // BGM
        Container.Bind<IBGMHandler>()
            .To<BGMHandler>()
            .FromComponentOn(m_CommonSystem)
            .AsSingle()
            .NonLazy();

        // カメラ
        Container.Bind<ICameraHandler>()
            .To<CameraHandler>()
            .FromComponentOn(m_CommonSystem)
            .AsSingle()
            .NonLazy();

        // Fade
        Container.Bind<IFadeManager>()
            .To<FadeManager>()
            .FromComponentOn(m_CommonUiSystem)
            .AsSingle()
            .NonLazy();

        // エフェクト保持
        Container.Bind(typeof(IEffectHolder), typeof(IInitializable))
            .To<EffectHolder>()
            .FromNew()
            .AsSingle()
            .NonLazy();

        // サウンド保持
        Container.Bind(typeof(ISoundHolder), typeof(IInitializable))
            .To<SoundHolder>()
            .FromNew()
            .AsSingle()
            .NonLazy();
    }
}
