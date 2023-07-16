using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Zenject;

public class HomeCharacterManager : MonoBehaviour
{
    [Inject]
    private DungeonProgressHolder m_DungeonProgressHolder;
    [Inject]
    private IConversationManager m_ConversationManager;
    [Inject]
    protected IInstantiater m_Instantiater;

    private List<ICollector> m_HomeCharacters = new List<ICollector>();

    private void Awake()
    {
        MessageBroker.Default.Receive<HomeCharacterInfo>().SubscribeWithState(this, (info, self) => self.RegisterConversation(info)).AddTo(this);
    }

    private void RegisterConversation(HomeCharacterInfo info)
    {
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

            var flowPrefab = info.Flows[i];
            var flow = m_Instantiater.InstantiatePrefab(flowPrefab).GetComponent<Fungus.Flowchart>();
            m_ConversationManager.Register(collector, flow, tf.position);
        }

        switch (m_DungeonProgressHolder.CurrentDungeonTheme)
        {
            case DUNGEON_THEME.GRASS:
                m_HomeCharacters[0].GetInterface<ICharaObjectHolder>().MoveObject.SetActive(false);
                break;

            case DUNGEON_THEME.PRIDE:
                m_HomeCharacters[1].GetInterface<ICharaObjectHolder>().MoveObject.SetActive(false);
                break;

            case DUNGEON_THEME.ROCK:
                m_HomeCharacters[2].GetInterface<ICharaObjectHolder>().MoveObject.SetActive(false);
                break;

            case DUNGEON_THEME.SNOW:
                m_HomeCharacters[3].GetInterface<ICharaObjectHolder>().MoveObject.SetActive(false);
                break;
        }
    }
}