using System.Collections;
using System.Collections.Generic;
using System.Text;
using Fungus;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public interface IBattleLogConfirmManager : IUiManager
{

}

public class BattleLogConfirmManager : MonoBehaviour, IBattleLogConfirmManager
{
    [Inject]
    private IBattleLogManager m_BattleLogManager;
    [Inject]
    protected ISoundHolder m_SoundHolder;
    [Inject]
    protected IInputManager m_InputManager;
    [Inject]
    protected IMenuUiManager m_MenuUiManager;
    [Inject]
    protected IMiniMapRenderer m_MiniMapRenderer;

    [SerializeField]
    private GameObject m_BigBattleLog;
    [SerializeField]
    private Text m_BattleLogText;

    private int m_CurrentIndex;
    private static readonly int MAX_LOG = 10;

    protected CompositeDisposable m_Disposables = new CompositeDisposable();
    CompositeDisposable IUiManager.Disposables => m_Disposables;

    /// <summary>
    /// Ui有効化
    /// </summary>
    void IUiManager.Activate()
    {
        m_BigBattleLog.SetActive(true);
        DisplayLog();

        // 入力購読
        var input = m_InputManager.InputStartEvent.SubscribeWithState(this, (input, self) => self.DetectInput(input.KeyCodeFlag));
        m_Disposables.Add(input);

        var closeUi = m_InputManager.SetActiveUi(this);
        m_Disposables.Add(closeUi); // 閉じるとき

        var disposable = m_MiniMapRenderer.SetActive(false);
        m_Disposables.Add(disposable);
    }

    /// <summary>
    /// Ui無効化
    /// </summary>
    private void Deactivate()
    {
        m_BigBattleLog.SetActive(false);
        m_CurrentIndex = 0;

        m_Disposables.Clear();

        m_MenuUiManager.Activate();
    }

    /// <summary>
    /// ログ表示
    /// </summary>
    private void DisplayLog()
    {
        var allLog = m_BattleLogManager.AllTextLog;

        var sb = new StringBuilder();
        for (int i = m_CurrentIndex + MAX_LOG; i >= m_CurrentIndex; i--)
        {
            int index = allLog.Length - 1 - i;
            if (index < 0 || index >= allLog.Length)
                continue;
            var text = allLog[index];
            sb.Append(text);
            if (i != m_CurrentIndex)
                sb.Append("\n");
        }

        m_BattleLogText.text = sb.ToString();
    }

    /// <summary>
    /// 入力受付
    /// </summary>
    /// <param name="flag"></param>
    private void DetectInput(KeyCodeFlag flag)
    {
        // Qで閉じる
        if (flag.HasBitFlag(KeyCodeFlag.Q))
        {
            Deactivate();

            if (m_SoundHolder.TryGetSound(UiManagerBase.QUIT, out var sound) == true)
                sound.Play();
            return;
        }

        //決定ボタン 該当メソッド実行
        if (flag.HasBitFlag(KeyCodeFlag.Return))
        {
            Deactivate();

            if (m_SoundHolder.TryGetSound(UiManagerBase.DECIDE, out var sound) == true)
                sound.Play();
            return;
        }

        //上にカーソル移動
        if (flag.HasBitFlag(KeyCodeFlag.W))
        {
            var allLog = m_BattleLogManager.AllTextLog;
            int logCount = allLog.Length;
            if (++m_CurrentIndex < 0)
                m_CurrentIndex = 0;
            else if (m_CurrentIndex >= logCount - MAX_LOG)
                m_CurrentIndex = Mathf.Max(0, logCount - MAX_LOG - 1);
            DisplayLog();
            return;
        }

        //下にカーソル移動
        if (flag.HasBitFlag(KeyCodeFlag.S))
        {
            var allLog = m_BattleLogManager.AllTextLog;
            int logCount = allLog.Length;
            if (--m_CurrentIndex < 0)
                m_CurrentIndex = 0;
            else if (m_CurrentIndex >= logCount - MAX_LOG)
                m_CurrentIndex = Mathf.Max(0, logCount - MAX_LOG - 1);
            DisplayLog();
            return;
        }
    }
}
