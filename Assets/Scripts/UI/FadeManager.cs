﻿using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System;
using DG.Tweening;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

public interface IFadeManager
{
    /// <summary>
    /// 暗転中にイベント
    /// </summary>
    /// <param name="whileEvent"></param>
    /// <returns></returns>
    Task StartFade<T>(T arg, Action<T> whileEvent);

    /// <summary>
    /// 暗転中にイベント
    /// </summary>
    /// <param name="whileEvent"></param>
    /// <param name="dungeonName"></param>
    /// <param name="where"></param>
    /// <returns></returns>
    Task StartFade<T>(T arg, Action<T> whileEvent, string dungeonName, string where);

    /// <summary>
    /// 暗転中にイベント
    /// </summary>
    /// <param name="whileEvent"></param>
    /// <returns></returns>
    Task StartFade<T, U>(T arg1, Action<T> whileEvent, U arg2, Action<U> completedEvent);

    /// <summary>
    /// 明転
    /// </summary>
    /// <param name="dungeonName"></param>
    /// <param name="where"></param>
    /// <returns></returns>
    Task TurnBright(string dungeonName, string where);

    /// <summary>
    /// 明転
    /// </summary>
    /// <param name="completedEvent"></param>
    /// <param name="dungeonName"></param>
    /// <param name="where"></param>
    /// <returns></returns>
    Task TurnBright<T>(T arg, Action<T> completedEvent);

    /// <summary>
    /// 明転終了後にイベント
    /// </summary>
    /// <param name="completedEvent"></param>
    /// <param name="dungeonName"></param>
    /// <param name="where"></param>
    /// <returns></returns>
    Task TurnBright<T>(T arg, Action<T> completedEvent, string dungeonName, string where);

    /// <summary>
    /// ホワイトアウト中にイベント
    /// </summary>
    /// <param name="whiteOutEvent"></param>
    /// <param name="dungeonName"></param>
    /// <param name="where"></param>
    /// <returns></returns>
    Task StartFadeWhite<T, U>(T arg1, Action<T> whileEvent, U arg2, Action<U> completedEvent);

    /// <summary>
    /// ホワイトアウト
    /// </summary>
    /// <param name="speed"></param>
    /// <param name="time"></param>
    /// <returns></returns>
    Task StartFadeWhite(float speed, float time);

    /// <summary>
    /// テキストのみ表示
    /// </summary>
    /// <param name="dungeonName"></param>
    /// <param name="where"></param>
    /// <returns></returns>
    Task ShowWhere(string dungeonName, string where);

    /// <summary>
    /// 暗転
    /// </summary>
    /// <returns></returns>
    Task BlackOut<T>(T arg, Action<T> completedEvent);

    /// <summary>
    /// 暗転中にシーンをロード
    /// </summary>
    /// <returns></returns>
    Task LoadScene(string sceneName);
}

public class FadeManager : MonoBehaviour, IFadeManager
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

    private void Awake()
    {
        MessageBroker.Default.Receive<WhiteOutMessage>().SubscribeWithState(this, async (message, self) => await self.StartFadeWhite(message.Speed, message.Time)).AddTo(this);
    }

    /// <summary>
    /// 暗転
    /// </summary>
    /// <param name="whileEvent"></param>
    /// <returns></returns>
    async Task IFadeManager.StartFade<T>(T arg, Action<T> whileEvent)
    {
        await FadeOutScreen(m_BlackScreen);
        whileEvent?.Invoke(arg);
        await FadeInScreen(m_BlackScreen);
    }

    /// <summary>
    /// 階移動暗転
    /// </summary>
    /// <param name="whileEvent"></param>
    /// <returns></returns>
    async Task IFadeManager.StartFade<T>(T arg, Action<T> whileEvent, string dungeonName, string where)
    {
        m_DungeonName.text = dungeonName;
        m_FloorText.text = where;

        await FadeOutScreen(m_BlackScreen);
        await FadeInText();
        whileEvent?.Invoke(arg);
        await FadeOutText();
        await FadeInScreen(m_BlackScreen);
    }

    /// <summary>
    /// 階移動暗転
    /// </summary>
    /// <param name="whileEvent"></param>
    /// <returns></returns>
    async Task IFadeManager.StartFade<T, U>(T arg1, Action<T> whileEvent, U arg2, Action<U> completedEvent)
    {
        await FadeOutScreen(m_BlackScreen);
        whileEvent?.Invoke(arg1);
        await FadeInScreen(m_BlackScreen);
        completedEvent?.Invoke(arg2);
    }

    /// <summary>
    /// チェックポイント -> ダンジョン
    /// </summary>
    /// <returns></returns>
    async Task IFadeManager.TurnBright(string dungeonName, string where)
    {
        m_DungeonName.text = dungeonName;
        m_FloorText.text = where;

        await m_BlackScreen.DOFade(1f, 0.001f).AsyncWaitForCompletion();
        await FadeInText();
        await FadeOutText();
        await FadeInScreen(m_BlackScreen);
    }

    /// <summary>
    /// 明転
    /// </summary>
    /// <returns></returns>
    async Task IFadeManager.TurnBright<T>(T arg, Action<T> completedEvent)
    {
        await m_BlackScreen.DOFade(1f, 0.001f).AsyncWaitForCompletion();
        await FadeInScreen(m_BlackScreen);
        completedEvent?.Invoke(arg);
    }

    /// <summary>
    /// ダンジョン -> チェックポイント
    /// </summary>
    /// <returns></returns>
    async Task IFadeManager.TurnBright<T>(T arg, Action<T> completedEvent, string dungeonName, string where)
    {
        m_DungeonName.text = dungeonName;
        m_FloorText.text = where;

        await m_BlackScreen.DOFade(1f, 0.001f).AsyncWaitForCompletion();
        await FadeInText();
        await FadeOutText();
        await FadeInScreen(m_BlackScreen);
        completedEvent?.Invoke(arg);
    }

    /// <summary>
    /// ホワイトアウト
    /// </summary>
    /// <returns></returns>
    private async Task StartFadeWhite(float speed, float time)
    {
        await FadeOutScreen(m_WhiteScreen, speed);
        await Task.Delay((int)(1000 * time));
        await FadeInScreen(m_WhiteScreen, speed);
    }
    Task IFadeManager.StartFadeWhite(float speed, float time) => StartFadeWhite(speed, time);

    /// <summary>
    /// ホワイトアウト
    /// </summary>
    /// <param name="whileEvent"></param>
    /// <param name="completedEvent"></param>
    /// <returns></returns>
    async Task IFadeManager.StartFadeWhite<T, U>(T arg1, Action<T> whileEvent, U arg2, Action<U> completedEvent)
    {
        await FadeOutScreen(m_WhiteScreen);
        whileEvent?.Invoke(arg1);
        await FadeInScreen(m_WhiteScreen);
        completedEvent?.Invoke(arg2);
    }

    /// <summary>
    /// テキストのみ表示
    /// </summary>
    /// <param name="dungeonName"></param>
    /// <param name="where"></param>
    /// <returns></returns>
    async Task IFadeManager.ShowWhere(string dungeonName, string where)
    {
        m_DungeonName.text = dungeonName;
        m_FloorText.text = where;

        await FadeInText();
        await FadeOutText();
    }

    /// <summary>
    /// 暗転のみ
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="arg"></param>
    /// <param name="completedEvent"></param>
    /// <returns></returns>
    async Task IFadeManager.BlackOut<T>(T arg, Action<T> completedEvent)
    {
        await FadeOutScreen(m_BlackScreen);
        completedEvent?.Invoke(arg);
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
        await FadeOutScreen(m_BlackScreen);

        // シーン切り替え
        task.allowSceneActivation = true;
    }

    /// <summary>
    /// スクリーン暗転
    /// </summary>
    private Task FadeOutScreen(Image screen, float speed = 1f) => screen.DOFade(1f, speed).AsyncWaitForCompletion();

    /// <summary>
    /// スクリーン明転
    /// </summary>
    private Task FadeInScreen(Image screen, float speed = 1f) => screen.DOFade(0f, speed).AsyncWaitForCompletion();

    /// <summary>
    /// テキスト表示
    /// </summary>
    private async Task FadeInText()
    {
        var text = m_FloorText.DOFade(1f, FADE_SPEED).AsyncWaitForCompletion();
        var name = m_DungeonName.DOFade(1f, FADE_SPEED).AsyncWaitForCompletion();
        await Task.WhenAll(text, name);
    }

    /// <summary>
    /// テキスト非表示
    /// </summary>
    private async Task FadeOutText()
    {
        var text = m_FloorText.DOFade(0f, FADE_SPEED).AsyncWaitForCompletion();
        var name = m_DungeonName.DOFade(0f, FADE_SPEED).AsyncWaitForCompletion();
        await Task.WhenAll(text, name);
    }
}
