using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System;
using DG.Tweening;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

public interface IFadeManager : ISingleton
{
    /// <summary>
    /// 暗転中にイベント
    /// </summary>
    /// <param name="blackOutEvent"></param>
    /// <param name="dungeonName"></param>
    /// <param name="where"></param>
    /// <returns></returns>
    Task StartFade(Action blackOutEvent, string dungeonName, string where);

    /// <summary>
    /// 明転
    /// </summary>
    /// <param name="dungeonName"></param>
    /// <param name="where"></param>
    /// <returns></returns>
    Task TurnBright(string dungeonName, string where);

    /// <summary>
    /// 明転終了後にイベント
    /// </summary>
    /// <param name="completeEvent"></param>
    /// <param name="dungeonName"></param>
    /// <param name="where"></param>
    /// <returns></returns>
    Task TurnBright(Action completeEvent, string dungeonName, string where);

    /// <summary>
    /// 暗転中にシーンをロード
    /// </summary>
    /// <returns></returns>
    Task LoadScene(string sceneName);
}

public class FadeManager : Singleton<FadeManager, IFadeManager>, IFadeManager
{
    /// <summary>
    /// 黒画面
    /// </summary>
    [SerializeField]
    private Image m_BlackScreen;

    /// <summary>
    /// 階層
    /// </summary>
    [SerializeField]
    private Text m_FloorText;

    /// <summary>
    /// ダンジョン名
    /// </summary>
    [SerializeField]
    private Text m_DungeonName;

    // フェイド速度
    private static readonly float FADE_SPEED = 1f;

    /// <summary>
    /// 階移動暗転
    /// </summary>
    /// <param name="blackOutEvent"></param>
    /// <returns></returns>
    async Task IFadeManager.StartFade(Action blackOutEvent, string dungeonName, string where)
    {
        m_DungeonName.text = dungeonName;
        m_FloorText.text = where;

        FadeOutScreen();
        await Task.Delay(1000);
        FadeInText();
        blackOutEvent?.Invoke();
        await Task.Delay(1000);
        FadeOutText();
        await Task.Delay(1000);
        FadeInScreen();
        await Task.Delay(1000);
    }

    /// <summary>
    /// チェックポイント -> ダンジョン
    /// </summary>
    /// <param name="blackOutEvent"></param>
    /// <returns></returns>
    async Task IFadeManager.TurnBright(string dungeonName, string where)
    {
        m_DungeonName.text = dungeonName;
        m_FloorText.text = where;
        m_BlackScreen.DOFade(1f, 0.001f);

        FadeInText();
        await Task.Delay(1000);
        FadeOutText();
        await Task.Delay(1000);
        FadeInScreen();
        await Task.Delay(1000);
    }

    /// <summary>
    /// ダンジョン -> チェックポイント
    /// </summary>
    /// <param name="blackOutEvent"></param>
    /// <returns></returns>
    async Task IFadeManager.TurnBright(Action completeEvent, string dungeonName, string where)
    {
        m_DungeonName.text = dungeonName;
        m_FloorText.text = where;
        m_BlackScreen.DOFade(1f, 0.001f);

        FadeInText();
        await Task.Delay(1000);
        FadeOutText();
        await Task.Delay(1000);
        FadeInScreen();
        await Task.Delay(1000);
        completeEvent?.Invoke();
    }

    /// <summary>
    /// 暗転中にシーンロード
    /// </summary>
    /// <param name="sceneName"></param>
    /// <returns></returns>
    async Task IFadeManager.LoadScene(string sceneName)
    {
        // 読み込み開始
        var task = SceneManager.LoadSceneAsync(sceneName);
        task.allowSceneActivation = false;

        // 暗転
        FadeOutScreen();
        await Task.Delay(1000);

        // シーン切り替え
        task.allowSceneActivation = true;
    }

    /// <summary>
    /// スクリーン暗転
    /// </summary>
    private void FadeOutScreen() => m_BlackScreen.DOFade(1f, FADE_SPEED);

    /// <summary>
    /// スクリーン明転
    /// </summary>
    private void FadeInScreen() => m_BlackScreen.DOFade(0f, FADE_SPEED);

    /// <summary>
    /// テキスト表示
    /// </summary>
    private void FadeInText()
    {
        m_FloorText.DOFade(1f, FADE_SPEED);
        m_DungeonName.DOFade(1f, FADE_SPEED);
    }

    /// <summary>
    /// テキスト非表示
    /// </summary>
    private void FadeOutText()
    {
        m_FloorText.DOFade(0f, FADE_SPEED);
        m_DungeonName.DOFade(0f, FADE_SPEED);
    }
}
