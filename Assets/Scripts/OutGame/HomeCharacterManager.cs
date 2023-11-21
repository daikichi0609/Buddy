using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Zenject;

public interface IHomeCharacterManager
{
    /// <summary>
    /// キャラ取得
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    ICollector GetHomeCharacter(int index);

    /// <summary>
    /// ホームキャラ非表示
    /// </summary>
    /// <returns></returns>
    IDisposable DeactivateFriends();
}

public class HomeCharacterManager : MonoBehaviour, IHomeCharacterManager
{
    [Inject]
    private InGameProgressHolder m_InGameInfoManager;
    [Inject]
    private IConversationManager m_ConversationManager;
    [Inject]
    protected IInstantiater m_Instantiater;

    private List<ICollector> m_HomeCharacters = new List<ICollector>();
    ICollector IHomeCharacterManager.GetHomeCharacter(int index) => m_HomeCharacters[index];

    private static readonly int FRIEND_COUNT = 3;

    private void Awake()
    {
        MessageBroker.Default.Receive<HomeCharacterInfo>().SubscribeWithState(this, (info, self) => self.RegisterConversation(info)).AddTo(this);
    }

    /// <summary>
    /// 会話登録
    /// </summary>
    /// <param name="info"></param>
    private void RegisterConversation(HomeCharacterInfo info)
    {
        int index = m_InGameInfoManager.FriendIndex;

        for (int i = 0; i < info.Prefabs.Length; i++)
        {
            var prefab = info.Prefabs[i];
            var chara = m_Instantiater.InstantiatePrefab(prefab, SceneInitializer.ms_OutGameUnitInjector);

            var tf = info.Transforms[i];
            chara.transform.position = tf.position;
            chara.transform.rotation = tf.rotation;

            ICollector collector = chara.GetComponent<ActorComponentCollector>();
            collector.Initialize();
            m_HomeCharacters.Add(collector);

            // 進行度に応じて会話登録せずに非表示にする
            if (index == i)
            {
                collector.GetInterface<ICharaObjectHolder>().MoveObject.SetActive(false);
                continue;
            }

            var flowPrefab = info.Flows[i];
            var flow = m_Instantiater.InstantiatePrefab(flowPrefab).GetComponent<Fungus.Flowchart>();
            m_ConversationManager.Register(collector, flow, tf.position);
        }
    }

    /// <summary>
    /// ホームキャラ非表示
    /// </summary>
    /// <param name="isActive"></param>
    IDisposable IHomeCharacterManager.DeactivateFriends()
    {
        for (int i = 0; i < FRIEND_COUNT; i++)
        {
            var chara = m_HomeCharacters[i];
            chara.GetInterface<ICharaObjectHolder>().MoveObject.SetActive(false);
        }

        int index = m_InGameInfoManager.FriendIndex;
        return Disposable.CreateWithState((m_HomeCharacters, index), tuple =>
        {
            for (int i = 0; i < FRIEND_COUNT; i++)
            {
                if (tuple.index == i)
                    continue;

                var chara = tuple.m_HomeCharacters[i];
                chara.GetInterface<ICharaObjectHolder>().MoveObject.SetActive(true);
            }
        });
    }
}