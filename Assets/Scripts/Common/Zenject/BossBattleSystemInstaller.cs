using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class BossBattleSystemInstaller : MonoInstaller
{
    [SerializeField]
    private GameObject m_BossBattleSystem;

    public override void InstallBindings()
    {
        // 初期化
        Container.Bind<ISceneInitializer>()
            .To<BossBattleInitializer>()
            .FromComponentOn(m_BossBattleSystem)
            .AsSingle()
            .NonLazy();

        // 会話システム
        Container.Bind<IConversationManager>()
            .To<ConversationManager>()
            .FromComponentOn(m_BossBattleSystem)
            .AsSingle()
            .NonLazy();
    }
}

