using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class FinalBossBattleSystemInstaller : MonoInstaller
{
    [SerializeField]
    private GameObject m_FinalBossBattleSystem;

    public override void InstallBindings()
    {
        // 初期化
        Container.Bind<ISceneInitializer>()
            .To<FinalBossBattleInitializer>()
            .FromComponentOn(m_FinalBossBattleSystem)
            .AsSingle()
            .NonLazy();

        // 会話システム
        Container.Bind<IConversationManager>()
            .To<ConversationManager>()
            .FromComponentOn(m_FinalBossBattleSystem)
            .AsSingle()
            .NonLazy();
    }
}