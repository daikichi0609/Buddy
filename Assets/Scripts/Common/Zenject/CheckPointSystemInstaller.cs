using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class CheckPointSystemInstaller : MonoInstaller
{
    [SerializeField]
    private GameObject m_CheckPointSystem;

    public override void InstallBindings()
    {
        // 初期化
        Container.Bind<ISceneInitializer>()
            .To<CheckPointInitializer>()
            .FromComponentOn(m_CheckPointSystem)
            .AsSingle()
            .NonLazy();

        // 会話システム
        Container.Bind<IConversationManager>()
            .To<ConversationManager>()
            .FromComponentOn(m_CheckPointSystem)
            .AsSingle()
            .NonLazy();

        // 会話システム
        Container.Bind<IObjectPoolController>()
            .To<ObjectPoolController>()
            .FromNew()
            .AsSingle()
            .NonLazy();
    }
}