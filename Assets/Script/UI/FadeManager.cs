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
    /// ホワイトアウト中にイベント
    /// </summary>
    /// <param name="whiteOutEvent"></param>
    /// <param name="dungeonName"></param>
    /// <param name="where"></param>
    /// <returns></returns>
    Task StartFadeWhite(Action whiteOutEvent);

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
    /// 白画面
    /// </summary>
    [SerializeField]
    private Image m_WhiteScreen;

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

        FadeOutScreen(m_BlackScreen);
        await Task.Delay(1000);
        FadeInText();
        blackOutEvent?.Invoke();
        await Task.Delay(1000);
        FadeOutText();
        await Task.Delay(1000);
        FadeInScreen(m_BlackScreen);
        await Task.Delay(1000);
    }

    /// <summary>
    /// 階移動暗転
    /// </summary>
    /// <param name="whiteOutEvent"></param>
    /// <returns></returns>
    async Task IFadeManager.StartFadeWhite(Action whiteOutEvent)
    {
        FadeOutScreen(m_WhiteScreen);
        await Task.Delay(1000);
        whiteOutEvent?.Invoke();
        await Task.Delay(1000);
        FadeInScreen(m_WhiteScreen);
        await Task.Delay(500);
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
        FadeInScreen(m_BlackScreen);
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
        FadeInScreen(m_BlackScreen);
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
        FadeOutScreen(m_BlackScreen);
        await Task.Delay(1000);

        // シーン切り替え
        task.allowSceneActivation = true;
    }

    /// <summary>
    /// スクリーン暗転
    /// </summary>
    private void FadeOutScreen(Image screen) => screen.DOFade(1f, FADE_SPEED);

    /// <summary>
    /// スクリーン明転
    /// </summary>
    private void FadeInScreen(Image screen) => screen.DOFade(0f, FADE_SPEED);

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
