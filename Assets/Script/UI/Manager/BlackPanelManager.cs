using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System;

public class BlackPanelManager : SingletonMonoBehaviour<BlackPanelManager>
{
    //暗転・明転動作完了フラグ
    public bool IsActive
    {
        get;
        set;
    } = false;

    //実行中リクエスト情報
    private Message.MRequestBlackPanel Request
    {
        get;
        set;
    }

    /// <summary>
    /// 暗転用パネルの透明度
    /// </summary>
    private ReactiveProperty<float> m_PanelAlpha = new ReactiveProperty<float>();

    public IObservable<float> GetPanelAlphaChanged
    {
        get { return m_PanelAlpha; }
    }

    private float PanelAlpha
    {
        get => m_PanelAlpha.Value;
        set => m_PanelAlpha.Value = value;
    }

    /// <summary>
    /// テキストの透明度
    /// </summary>
    private ReactiveProperty<float> m_TextAlpha = new ReactiveProperty<float>();

    public IObservable<float> GetTextAlphaChanged
    {
        get { return m_TextAlpha; }
    }

    private float TextAlpha
    {
        get => m_TextAlpha.Value;
        set => m_TextAlpha.Value = value;
    }

    //暗転速度
    private float BlackPanelSpeed
    {
        get;
    } = 0.1f;

    //テキスト透明度変化速度
    private float TextSpeed
    {
        get;
    } = 0.05f;

    protected override void Awake()
    {
        GameManager.Instance.GetUpdate
            .Where(_ => IsActive == true)
            .Subscribe(_ => ControllBlackPanel());

        MessageBroker.Default.Receive<Message.MRequestBlackPanel>().Subscribe(_ => ReceiveRequest(_)).AddTo(this);

        GetPanelAlphaChanged.Subscribe(_ => PanelUpdate()).AddTo(this);
        GetTextAlphaChanged.Subscribe(_ => TextUpdate()).AddTo(this);
    }

    private void ReceiveRequest(Message.MRequestBlackPanel request)
    {
        IsActive = true;
        Request = request;
    }

    private void ControllBlackPanel()
    {
        bool isDark = Request.IsDark;

        switch(isDark)
        {
            case true:
                PanelAlpha += BlackPanelSpeed;
                if (PanelAlpha >= 1f)
                {
                    PanelAlpha = 1f;
                    IsActive = false;
                    MessageBroker.Default.Publish(new Message.MFinishBlackPanel { IsDark = true });
                }
                break;

            case false:
                PanelAlpha -= BlackPanelSpeed;
                if (PanelAlpha >= 0f)
                {
                    PanelAlpha = 0f;
                    IsActive = false;
                    MessageBroker.Default.Publish(new Message.MFinishBlackPanel { IsDark = false });
                }
                break;
        }
    }

    public void ControllText()
    {
        bool s = false;

        if(s == false)
        {
            TextAlpha += TextSpeed;
            if(TextAlpha >= 1f)
            {
                s = true;
            }
        }
        else
        {
            TextAlpha -= TextSpeed;
            if(TextAlpha <= 0f)
            {
                MessageBroker.Default.Publish(new Message.MFinishFloorText());
            }
        }
    }

    private void PanelUpdate()
    {
        if (PanelAlpha > 1f)
        {
            PanelAlpha = 1f;
        }

        if (PanelAlpha < 0f)
        {
            PanelAlpha = 0f;
        }

        UiHolder.Instance.BlackPanel.GetComponent<Image>().color = new Color(0, 0, 0, PanelAlpha);
    }

    private void TextUpdate()
    {
        if (TextAlpha > 1f)
        {
            TextAlpha = 1f;
        }

        if (TextAlpha < 0f)
        {
            TextAlpha = 0f;
        }

        UiHolder.Instance.FloorNumText.color = new Color(0, 0, 0, TextAlpha);
    }
}
