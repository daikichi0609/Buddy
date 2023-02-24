using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using UnityEngine.UI;

/// <summary>
/// バッグUi
/// </summary>
public class BagUiManager : UiManagerBase<BagUiManager, IUiManager>, IUiManager
{
    private BagUiManager.BagUi m_UiInterface = new BagUi();
    protected override IUiBase UiInterface => m_UiInterface;

    protected override OptionElement CreateOptionElement()
    {
        return new OptionElement
        (new Action[0] {},
        new string[0] {});
    }

    public class BagUi : UiBase
    {
        /// <summary>
        /// 操作するUi
        /// </summary>
        protected override GameObject Ui => UiHolder.Instance.BagUi;

        /// <summary>
        /// 操作するテキストUi
        /// </summary>
        protected override List<Text> Texts => UiHolder.Instance.ItemTexts;
    }
}