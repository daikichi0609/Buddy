using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using UnityEngine.UI;
using Zenject;

/// <summary>
/// 選択肢Ui作成時の情報
/// </summary>
public readonly struct OptionElement
{
    /// <summary>
    /// 選択肢メソッド
    /// </summary>
    public Subject<int> OptionMethod { get; }

    /// <summary>
    /// 選択肢テキスト
    /// </summary>
    public string[] OptionTexts { get; }

    /// <summary>
    /// 選択肢メソッドの数
    /// </summary>
    public int MethodCount { get; }

    public OptionElement(Subject<int> method, string[] texts)
    {
        OptionMethod = method;
        OptionTexts = texts;
        MethodCount = OptionTexts.Length;
    }

    public OptionElement(Subject<int> method, string[] texts, int methodCount)
    {
        OptionMethod = method;
        OptionTexts = texts;
        MethodCount = methodCount;
    }
}

public interface IUiManager
{
    /// <summary>
    /// 親Ui
    /// </summary>
    IUiManager ParentUi { set; }

    /// <summary>
    /// 入力購読終わり
    /// </summary>
    CompositeDisposable Disposables { get; }

    /// <summary>
    /// UI表示
    /// </summary>
    void Activate();

    /// <summary>
    /// UI表示
    /// </summary>
    void Activate(IUiManager parent);

    /// <summary>
    /// UI非表示
    /// </summary>
    void Deactivate();

    /// <summary>
    /// 親Ui含めUI非表示
    /// </summary>
    void DeactivateAll();
}

/// <summary>
/// 左上メニューと説明のUi
/// </summary>
public abstract class UiManagerBase : MonoBehaviour, IUiManager
{
    [Inject]
    protected IInputManager m_InputManager;
    [Inject]
    protected ITurnManager m_TurnManager;
    [Inject]
    protected IBattleLogManager m_BattleLogManager;
    [Inject]
    protected ISoundHolder m_SoundHolder;

    protected static readonly string DECIDE = "UiDecide";
    private static readonly string MOVE = "UiMove";
    private static readonly string QUIT = "UiQuit";

    /// <summary>
    /// Ui操作インターフェイス
    /// </summary>
    protected abstract IUiBase CurrentUiInterface { get; }
    protected ReactiveProperty<int> m_Depth = new ReactiveProperty<int>();

    /// <summary>
    /// 説明文
    /// </summary>
    protected abstract string FixLogText { get; }

    /// <summary>
    /// 選択肢の購読と選択肢
    /// </summary>
    protected abstract OptionElement[] CreateOptionElement();

    /// <summary>
    /// 選択肢メソッド
    /// </summary>
    protected abstract Subject<int> CurrentOptionSubject { get; }

    /// <summary>
    /// 選択肢Idの変動
    /// </summary>
    private Subject<int> m_OnOptionIdChange = new Subject<int>();
    protected IObservable<int> OnOptionIdChange => m_OnOptionIdChange;

    /// <summary>
    /// 親Ui
    /// </summary>
    private IUiManager m_ParentUi;
    IUiManager IUiManager.ParentUi { set => m_ParentUi = value; }

    /// <summary>
    /// Deactive時
    /// </summary>
    protected CompositeDisposable m_Disposables = new CompositeDisposable();
    CompositeDisposable IUiManager.Disposables => m_Disposables;

    protected virtual void Awake()
    {
        m_OnOptionIdChange.SubscribeWithState(this, (_, self) =>
        {
            if (self.m_SoundHolder.TryGetSound(MOVE, out var sound) == true)
                sound.Play();
        }).AddTo(this);
    }

    /// <summary>
    /// 入力受付
    /// </summary>
    /// <param name="flag"></param>
    private void DetectInput(KeyCodeFlag flag)
    {
        if (m_TurnManager.NoOneActing == false)
            return;

        // Qで閉じる
        if (flag.HasBitFlag(KeyCodeFlag.Q))
        {
            if (m_Depth.Value > 0)
                m_Depth.Value--;
            else
                Deactivate();

            if (m_SoundHolder.TryGetSound(QUIT, out var sound) == true)
                sound.Play();
            return;
        }

        //決定ボタン 該当メソッド実行
        if (flag.HasBitFlag(KeyCodeFlag.Return))
        {
            CurrentUiInterface.InvokeOptionMethod();

            if (m_SoundHolder.TryGetSound(DECIDE, out var sound) == true)
                sound.Play();
            return;
        }

        //上にカーソル移動
        if (flag.HasBitFlag(KeyCodeFlag.W))
        {
            var id = CurrentUiInterface.AddOptionId(-1);
            m_OnOptionIdChange.OnNext(id);
            return;
        }

        //下にカーソル移動
        if (flag.HasBitFlag(KeyCodeFlag.S))
        {
            var id = CurrentUiInterface.AddOptionId(1);
            m_OnOptionIdChange.OnNext(id);
            return;
        }
    }

    /// <summary>
    /// Ui有効化
    /// </summary>
    protected void Activate(IUiManager parent)
    {
        m_ParentUi = parent; // 親Uiセット
        m_ParentUi.Disposables.Clear();
        Activate();
    }
    void IUiManager.Activate(IUiManager parent) => Activate(parent);
    protected void Activate()
    {
        // 入力購読
        var input = m_InputManager.InputStartEvent.SubscribeWithState(this, (input, self) => self.DetectInput(input.KeyCodeFlag));
        m_Disposables.Add(input);

        var closeUi = m_InputManager.SetActiveUi(this);
        m_Disposables.Add(closeUi); // 閉じるとき

        InitializeUi(); // Uiの初期化

        // バトルログで説明
        if (FixLogText != string.Empty)
        {
            var log = m_BattleLogManager.FixLogForUi(FixLogText);
            m_Disposables.Add(log);
        }
        else
            m_BattleLogManager.Deactive();
    }
    void IUiManager.Activate() => Activate();

    /// <summary>
    /// Ui無効化
    /// </summary>
    protected void Deactivate(bool openParent = true)
    {
        FinalizeUi();

        // 入力購読終わり
        m_Disposables.Clear();

        // 親Uiがあるなら操作可能に
        if (m_ParentUi != null)
        {
            if (openParent == true)
                m_ParentUi.Activate();

            m_ParentUi = null;
        }
    }
    void IUiManager.Deactivate() => Deactivate();

    /// <summary>
    /// 親Ui含め無効化
    /// </summary>
    protected void DeactivateAll()
    {
        if (m_ParentUi != null)
            m_ParentUi.DeactivateAll();

        Deactivate(false);
    }
    void IUiManager.DeactivateAll() => DeactivateAll();

    /// <summary>
    /// Uiの初期化
    /// 操作するUiが一つの場合
    /// </summary>
    protected virtual void InitializeUi()
    {
        // 初期化
        var option = CreateOptionElement();

        CurrentUiInterface.Initialize(m_Disposables, option[0]);
        CurrentUiInterface.SetActive(true); // 表示
    }

    /// <summary>
    /// Uiの終了時
    /// 操作するUiが一つの場合
    /// </summary>
    protected virtual void FinalizeUi()
    {
        // Ui非表示
        CurrentUiInterface.SetActive(false);
    }
}