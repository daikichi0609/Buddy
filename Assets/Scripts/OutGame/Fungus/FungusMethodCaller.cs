using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Zenject;

public class FungusMethodCaller : MonoBehaviour
{
    [Inject]
    private IFadeManager m_FadeManager;
    [Inject]
    private ISceneInitializer m_SceneInitializer;
    [Inject]
    private IConversationManager m_ConversationManager;

    /// <summary>
    /// InitializerのFungus用メソッド呼び出し
    /// </summary>
    public void CallInitializerFungusMethod() => m_SceneInitializer.FungusMethod();

    /// <summary>
    /// 会話終了時
    /// </summary>
    public void OnFinishFlow() => m_ConversationManager.OnFinishTalking();

    /// <summary>
    /// ダンジョンシーンをロード
    /// </summary>
    public void LoadDungeonScene() => m_FadeManager.LoadScene(SceneName.SCENE_DUNGEON);

    /// <summary>
    ///  ホームシーンをロード
    /// </summary>
    public void LoadHomeScene() => m_FadeManager.LoadScene(SceneName.SCENE_HOME);

    /// <summary>
    /// フレンド決定
    /// </summary>
    public void SetFriendRagon() => MessageBroker.Default.Publish(new SetFriendMessage(CHARA_NAME.RAGON));
    public void SetFriendBerry() => MessageBroker.Default.Publish(new SetFriendMessage(CHARA_NAME.BERRY));
    public void SetFriendDorch() => MessageBroker.Default.Publish(new SetFriendMessage(CHARA_NAME.DORCHE));
}

public readonly struct SetFriendMessage
{
    public SetFriendMessage(CHARA_NAME name)
    {
        FriendName = name;
    }

    public CHARA_NAME FriendName { get; }
}