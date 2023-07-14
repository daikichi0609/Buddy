using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using NaughtyAttributes;
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
[Serializable]
public abstract class UiManagerBase : MonoBehaviour, IUiManager
{
    [Inject]
    protected IInputManager m_InputManager;
    [Inject]
    protected IBattleLogManager m_BattleLogManager;
    [Inject]
    protected ISoundHolder m_SoundHolder;
    [Inject]
    protected IMiniMapRenderer m_MiniMapRenderer;
    [Inject]
    protected IUnitHolder m_UnitHolder;

    protected static readonly string DECIDE = "UiDecide";
    private static readonly string MOVE = "UiMove";
    private static readonly string QUIT = "UiQuit";

    /// <summary>
    /// ミニマップを表示するか
    /// </summary>
    protected abstract bool IsActiveMiniMap { get; }

    /// <summary>
    /// 操作するUi
    /// </summary>
    [SerializeField, ReorderableList]
    protected UiBase[] m_UiManaged;
    protected IUiBase CurrentUi => m_UiManaged[m_Depth.Value];
    protected ReactiveProperty<int> m_Depth = new ReactiveProperty<int>();
    [ShowNativeProperty]
    protected abstract int MaxDepth { get; }

    /// <summary>
    /// 選択肢イベント
    /// </summary>
    protected Subject<int>[] m_OptionMethods;
    protected Subject<int> CurrentOptionSubject => m_OptionMethods[m_Depth.Value];

    /// <summary>
    /// 選択肢Idの変動
    /// </summary>
    private Subject<int> m_OnOptionIdChange = new Subject<int>();
    protected IObservable<int> OnOptionIdChange => m_OnOptionIdChange;

    /// <summary>
    /// 選択肢の購読と選択肢
    /// </summary>
    protected abstract OptionElement[] CreateOptionElement();

    /// <summary>
    /// 説明文
    /// </summary>
    protected abstract string FixLogText { get; }

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
        m_OptionMethods = new Subject<int>[MaxDepth];
        for (int i = 0; i < MaxDepth; i++)
            m_OptionMethods[i] = new Subject<int>();

        m_OnOptionIdChange.SubscribeWithState(this, (_, self) =>
        {
            if (self.m_SoundHolder.TryGetSound(MOVE, out var sound) == true)
                sound.Play();
        }).AddTo(this);
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

        // ミニマップの非表示
        if (IsActiveMiniMap == false)
        {
            var disposable = m_MiniMapRenderer.SetActive(false);
            m_Disposables.Add(disposable);
        }
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
    /// </summary>
    protected void InitializeUi()
    {
        // 初期化
        var option = CreateOptionElement();

        for (int i = 0; i < m_UiManaged.Length; i++)
        {
            IUiBase ui = m_UiManaged[i];
            bool changeColor = i == 0 ? true : false;
            ui.Initialize(m_Disposables, option[i], changeColor);
            ui.SetActive(true); // 表示
        }
    }

    /// <summary>
    /// Uiの終了時
    /// </summary>
    private void FinalizeUi()
    {
        // Ui非表示
        for (int i = 0; i < m_UiManaged.Length; i++)
        {
            IUiBase ui = m_UiManaged[i];
            ui.SetActive(false); // 非表示
        }
    }

    /// <summary>
    /// 入力受付
    /// </summary>
    /// <param name="flag"></param>
    private void DetectInput(KeyCodeFlag flag)
    {
        if (m_UnitHolder.NoOneActing == false)
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
            CurrentUi.InvokeOptionMethod();

            if (m_SoundHolder.TryGetSound(DECIDE, out var sound) == true)
                sound.Play();
            return;
        }

        //上にカーソル移動
        if (flag.HasBitFlag(KeyCodeFlag.W))
        {
            var id = CurrentUi.AddOptionId(-1);
            m_OnOptionIdChange.OnNext(id);
            return;
        }

        //下にカーソル移動
        if (flag.HasBitFlag(KeyCodeFlag.S))
        {
            var id = CurrentUi.AddOptionId(1);
            m_OnOptionIdChange.OnNext(id);
            return;
        }
    }
}