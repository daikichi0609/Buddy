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

    /// <summary>
    /// 親Ui
    /// </summary>
    IUiManager ParentUi { set; }

    /// <summary>
    /// 入力購読終わり
    /// </summary>
    CompositeDisposable Disposables { get; }
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

    /// <summary>
    /// Ui操作インターフェイス
    /// </summary>
    protected abstract IUiBase UiInterface { get; }

    /// <summary>
    /// 選択肢の購読と選択肢
    /// </summary>
    protected abstract OptionElement CreateOptionElement();

    /// <summary>
    /// 選択肢メソッド
    /// </summary>
    protected Subject<int> m_OptionMethod = new Subject<int>();

    /// <summary>
    /// Deactive時
    /// </summary>
    protected CompositeDisposable m_Disposables = new CompositeDisposable();
    CompositeDisposable IUiManager.Disposables => m_Disposables;

    /// <summary>
    /// 親Ui
    /// </summary>
    private IUiManager m_ParentUi;
    IUiManager IUiManager.ParentUi { set => m_ParentUi = value; }

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
            Deactivate();
            return;
        }

        //決定ボタン 該当メソッド実行
        if (flag.HasBitFlag(KeyCodeFlag.Return))
        {
            UiInterface.InvokeOptionMethod();
            return;
        }

        //上にカーソル移動
        if (flag.HasBitFlag(KeyCodeFlag.W))
        {
            UiInterface.AddOptionId(-1);
            return;
        }

        //下にカーソル移動
        if (flag.HasBitFlag(KeyCodeFlag.S))
        {
            UiInterface.AddOptionId(1);
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

        var closeUi = m_InputManager.SetActiveUi(this.UiInterface);
        m_Disposables.Add(closeUi); // 閉じるとき

        // 初期化
        var option = CreateOptionElement();
        UiInterface.Initialize(m_Disposables, option);

        UiInterface.SetActive(true); // 表示

        // バトルログは消す
        m_BattleLogManager.Deactive();

        // キャラの行動を許可しない
        var disposable = m_TurnManager.RequestProhibitAction(null);
        m_Disposables.Add(disposable);
    }
    void IUiManager.Activate() => Activate();

    /// <summary>
    /// Ui無効化
    /// </summary>
    protected void Deactivate(bool openParent = true)
    {
        // Ui非表示
        UiInterface.SetActive(false);

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
}