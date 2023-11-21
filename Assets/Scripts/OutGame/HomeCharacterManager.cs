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
    IDisposable DeactivateAll();
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
            // 進行度に応じて生成しない
            if (index == i)
                continue;

            var prefab = info.Prefabs[i];
            var chara = m_Instantiater.InstantiatePrefab(prefab, SceneInitializer.ms_OutGameUnitInjector);

            var tf = info.Transforms[i];
            chara.transform.position = tf.position;
            chara.transform.rotation = tf.rotation;

            ICollector collector = chara.GetComponent<ActorComponentCollector>();
            collector.Initialize();
            m_HomeCharacters.Add(collector);

            var flowPrefab = info.Flows[i];
            var flow = m_Instantiater.InstantiatePrefab(flowPrefab).GetComponent<Fungus.Flowchart>();
            m_ConversationManager.Register(collector, flow, tf.position);
        }
    }

    /// <summary>
    /// ホームキャラ非表示
    /// </summary>
    /// <param name="isActive"></param>
    IDisposable IHomeCharacterManager.DeactivateAll()
    {
        foreach (var chara in m_HomeCharacters)
            chara.GetInterface<ICharaObjectHolder>().MoveObject.SetActive(false);

        return Disposable.CreateWithState(m_HomeCharacters, characters =>
        {
            foreach (var chara in characters)
                chara.GetInterface<ICharaObjectHolder>().MoveObject.SetActive(true);
        });
    }
}