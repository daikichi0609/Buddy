using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System;
using DG.Tweening;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public sealed class CropInfo
{
    public float CurrentTimer { get; set; }

    public double Duration { get; }
    public string CropText { get; }

    public CropInfo(CropMessage message)
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

    private List<CropInfo> m_Dialogs = new List<CropInfo>();

    private void Awake()
    {
        MessageBroker.Default.Receive<CropMessage>().SubscribeWithState(this, (message, self) => self.m_Dialogs.Add(new CropInfo(message))).AddTo(this);
    }

    private void Update()
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
}
