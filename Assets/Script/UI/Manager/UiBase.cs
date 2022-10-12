using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using UnityEngine.UI;

public abstract class UiBase
{
    /// <summary>
    /// Ui表示中かどうか
    /// </summary>
    private ReactiveProperty<bool> m_IsActive = new ReactiveProperty<bool>(false);

    public IObservable<bool> IsActiveChanged
    {
        get => m_IsActive.Skip(1);
    }

    public bool IsActive
    {
        get => m_IsActive.Value;
        set => m_IsActive.Value = value;
    }

    /// <summary>
    /// 選択肢Id
    /// </summary>
    private ReactiveProperty<int> m_OptionId = new ReactiveProperty<int>();

    public IObservable<int> GetOptionId
    {
        get => m_OptionId.Skip(1);
    }

    public int OptionId
    {
        get => m_OptionId.Value;
        set => m_OptionId.Value = value;
    }

    /// <summary>
    /// 選択肢の数
    /// </summary>
    protected virtual int OptionCount
    {
        get;
        set;
    }

    /// <summary>
    /// 選択肢のメソッド
    /// </summary>
    protected virtual List<Action> OptionMethods
    {
        get;
    }

    /// <summary>
    /// 操作するUi
    /// </summary>
    protected virtual GameObject Ui
    {
        get;
    }

    /// <summary>
    /// 操作するテキストUi
    /// </summary>
    protected virtual List<Text> Texts
    {
        get;
    }

    /// <summary>
    /// Ui表示・非表示操作 Subscribeするもの
    /// </summary>
    public void SwitchUi()
    {
        InputManager.Instance.IsProhibitDuplicateInput = true;
        Ui.SetActive(IsActive);

        if(IsActive == true)
        {
            UpdateText();
        }
    }

    /// <summary>
    /// 入力検知
    /// </summary>
    public virtual void DetectInput()
    {
        //Ui表示中じゃないなら受け付けない
        if (IsActive == false)
        {
            return;
        }

        if(InputManager.Instance.IsProhibitDuplicateInput == true)
        {
            InputManager.Instance.IsProhibitDuplicateInput = false;
            return;
        }

        //Uiを閉じる
        if (Input.GetKeyDown(KeyCode.Q))
        {
            CloseUi();
            return;
        }

        //決定ボタン 該当メソッド実行
        if (Input.GetKeyDown(KeyCode.Return))
        {
            IsActive = false;
            OptionMethods[OptionId]();
            return;
        }

        //上にカーソル移動
        if (Input.GetKeyDown(KeyCode.W))
        {
            if (OptionId >= 1)
            {
                OptionId--;
            }
        }

        //下にカーソル移動
        if (Input.GetKeyDown(KeyCode.S))
        {
            if (OptionId <= Texts.Count - 2)
            {
                OptionId++;
            }
        }
    }

    /// <summary>
    /// テキスト更新 Subscribeするもの
    /// </summary>
    public virtual void UpdateText()
    {
        //選択肢の文字色更新
        for (int i = 0; i <= Texts.Count - 1; i++)
        {
            Texts[i].color = Color.white;
        }

        //選択中の文字色更新
        Texts[OptionId].color = Color.yellow;
    }

    /// <summary>
    /// Uiを閉じる
    /// </summary>
    protected virtual void CloseUi()
    {
        InputManager.Instance.IsProhibitDuplicateInput = true;
        OptionId = 0;
        IsActive = false;
    }
}
