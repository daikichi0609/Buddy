using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System;
using DG.Tweening;
using System.Threading.Tasks;

public interface IFadeManager : ISingleton
{
    Task NextFloor(Action blackOutEvent);
}

public class FadeManager : Singleton<FadeManager, IFadeManager>, IFadeManager
{
    /// <summary>
    /// 黒画面
    /// </summary>
    private Image BlackScreen => UiHolder.Instance.BlackPanel.GetComponent<Image>();

    /// <summary>
    /// 階層
    /// </summary>
    private Text FloorText => UiHolder.Instance.FloorNumText;

    // フェイド速度
    private static readonly float FADE_SPEED = 1f;

    protected override void Awake()
    {
        base.Awake();
    }

    /// <summary>
    /// 階移動暗転
    /// </summary>
    /// <param name="blackOutEvent"></param>
    /// <returns></returns>
    async Task IFadeManager.NextFloor(Action blackOutEvent)
    {
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

    private void FadeOutScreen() => BlackScreen.DOFade(1f, FADE_SPEED);
    private void FadeInScreen() => BlackScreen.DOFade(0f, FADE_SPEED);

    private void FadeOutText() => FloorText.DOFade(0f, FADE_SPEED);
    private void FadeInText() => FloorText.DOFade(1f, FADE_SPEED);
}
