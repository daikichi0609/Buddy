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
    /// チェックポイント到達会話フロー終了時
    /// </summary>
    public void OnFinishCheckPointFlow() => m_FadeManager.StartFade(() => m_SceneInitializer.ReadyToOperatable(), string.Empty, string.Empty);

    /// <summary>
    /// チェックポイント到達会話フロー終了時
    /// </summary>
    public void OnFinishBossBattleFlow() => m_FadeManager.StartFadeWhite(() => m_SceneInitializer.ReadyToOperatable());

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
