using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System.Collections.Generic;
using NaughtyAttributes;
using System;

public sealed class CropInfo
{
    public float CurrentTimer { get; set; }

    public double Duration { get; }
    public string CropText { get; }

    public CropInfo(CropTextMessage message)
    {
        Duration = message.Duration;
        CropText = message.Text;
    }
}

public interface ITimelineCropManager
{

}

public class TimelineCropManager : MonoBehaviour, ITimelineCropManager
{
    [SerializeField]
    private GameObject m_Panel;
    [SerializeField]
    private Text m_Text;

    /// <summary>
    /// 表示するクロップテキスト
    /// </summary>
    private List<CropInfo> m_Dialogs = new List<CropInfo>();

    /// <summary>
    /// 有効時にクロップ表示
    /// </summary>
    private ReactiveProperty<bool> m_IsActive = new ReactiveProperty<bool>();

    private void Awake()
    {
        MessageBroker.Default.Receive<CropTextMessage>().SubscribeWithState(this, (message, self) => self.m_Dialogs.Add(new CropInfo(message))).AddTo(this);
        MessageBroker.Default.Receive<CropSetActivateMessage>().SubscribeWithState(this, (message, self) => self.m_IsActive.Value = message.IsActive).AddTo(this);
        m_IsActive.SubscribeWithState(this, (isActive, self) =>
        {
            if (isActive == false)
                self.DeactivateCrop();
        }).AddTo(this);
    }

    private void Update()
    {
        // クロップ表示
        if (m_IsActive.Value == true)
            ShowCrop();
    }

    /// <summary>
    /// クロップ表示
    /// </summary>
    private void ShowCrop()
    {
        for (int i = 0; i < m_Dialogs.Count; i++)
        {
            var timer = m_Dialogs[i];
            timer.CurrentTimer += Time.deltaTime;
            if (timer.CurrentTimer >= timer.Duration)
                m_Dialogs.Remove(timer);
        }

        if (m_Dialogs.Count > 0)
        {
            m_Panel.SetActive(true);
            m_Text.text = m_Dialogs[0].CropText;
        }
        else
        {
            m_Panel.SetActive(false);
            m_Text.text = string.Empty;
        }
    }

    /// <summary>
    /// クロップ非表示
    /// </summary>
    private void DeactivateCrop()
    {
        m_Dialogs.Clear();
        m_Panel.SetActive(false);
        m_Text.text = string.Empty;
    }
}
