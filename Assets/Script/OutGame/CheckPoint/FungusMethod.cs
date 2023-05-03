using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FungusMethod : MonoBehaviour
{
    /// <summary>
    /// チェックポイント到達会話フロー終了時
    /// </summary>
    public void OnFinishCheckPointFlow() => FadeManager.Interface.StartFade(() => CheckPointController.Instance.ReadyToOperatable(), string.Empty, string.Empty);

    /// <summary>
    /// ダンジョンシーンをロード
    /// </summary>
    public void LoadDungeonScene() => FadeManager.Interface.LoadScene(SceneName.SCENE_DUNGEON);

    /// <summary>
    /// 会話終了時
    /// </summary>
    public void OnFinishFlow() => ConversationManager.Interface.OnFinishTalking();
}
