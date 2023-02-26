using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System;
using DG.Tweening;
using System.Threading.Tasks;

public interface IFadeManager : ISingleton
{
    Task NextFloor(Action blackOutEvent, int nextFloor, string dungeonName);
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

    protected override void Awake()
    {
        base.Awake();
    }

    /// <summary>
    /// 階移動暗転
    /// </summary>
    /// <param name="blackOutEvent"></param>
    /// <returns></returns>
    async Task IFadeManager.NextFloor(Action blackOutEvent, int nextFloor, string dungeonName)
    {
        m_FloorText.text = nextFloor.ToString() + "F";
        m_DungeonName.text = dungeonName;

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

    private void FadeOutScreen() => m_BlackScreen.DOFade(1f, FADE_SPEED);
    private void FadeInScreen() => m_BlackScreen.DOFade(0f, FADE_SPEED);

    private void FadeInText()
    {
        m_FloorText.DOFade(1f, FADE_SPEED);
        m_DungeonName.DOFade(1f, FADE_SPEED);
    }
    private void FadeOutText()
    {
        m_FloorText.DOFade(0f, FADE_SPEED);
        m_DungeonName.DOFade(0f, FADE_SPEED);
    }
}
