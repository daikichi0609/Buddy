using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System;
using DG.Tweening;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public sealed class DialogTimer
{
    public float CurrentTimer { get; set; }
    public DialogMessage DialogMessage { get; }

    public DialogTimer(DialogMessage message) => DialogMessage = message;
}

public interface ITimelineDialogManager
{

}

public class TimelineDialogManager : MonoBehaviour, ITimelineDialogManager
{
    [SerializeField]
    private GameObject m_Panel;
    [SerializeField]
    private Text m_Text;

    private List<DialogTimer> m_Dialogs = new List<DialogTimer>();

    private void Awake()
    {
        MessageBroker.Default.Receive<DialogMessage>().SubscribeWithState(this, (message, self) => self.m_Dialogs.Add(new DialogTimer(message))).AddTo(this);
    }

    private void Update()
    {
        for (int i = 0; i < m_Dialogs.Count; i++)
        {
            var timer = m_Dialogs[i];
            timer.CurrentTimer += Time.deltaTime;
            if (timer.CurrentTimer >= timer.DialogMessage.Dialog.m_Time)
                m_Dialogs.Remove(timer);
        }

        if (m_Dialogs.Count > 0)
        {
            m_Panel.SetActive(true);
            m_Text.text = m_Dialogs[0].DialogMessage.Dialog.m_Text;
        }
        else
        {
            m_Panel.SetActive(false);
            m_Text.text = string.Empty;
        }
    }
}
