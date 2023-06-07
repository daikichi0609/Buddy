using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class FungusMethod : MonoBehaviour
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
    public async void CallInitializerFungusMethod() => await m_SceneInitializer.FungusMethod();

    /// <summary>
    /// ダンジョンシーンをロード
    /// </summary>
    public void LoadDungeonScene() => m_FadeManager.LoadScene(SceneName.SCENE_DUNGEON);

    /// <summary>
    ///  ホームシーンをロード
    /// </summary>
    public void LoadHomeScene() => m_FadeManager.LoadScene(SceneName.SCENE_HOME);

    /// <summary>
    /// 会話終了時
    /// </summary>
    public void OnFinishFlow() => m_ConversationManager.OnFinishTalking();
}
