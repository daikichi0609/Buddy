using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using Zenject;
using System;

public interface IBattleLogManager
{
    /// <summary>
    /// ログを出す
    /// </summary>
    /// <param name="log"></param>
    void Log(string log);

    /// <summary>
    /// ログを非表示
    /// </summary>
    void Deactive();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="log"></param>
    /// <returns></returns>
    IDisposable FixLogForUi(string log);

    /// <summary>
    /// ログ全部
    /// </summary>
    List<string> AllTextLog { get; }
}

public class BattleLogManager : MonoBehaviour, IBattleLogManager
{
    [SerializeField]
    private Text m_Text;

    [SerializeField]
    private GameObject m_BattleLog;

    /// <summary>
    /// 全てのログ
    /// </summary>
    private List<string> m_AllTextLog = new List<string>();
    List<string> IBattleLogManager.AllTextLog => m_AllTextLog;

    /// <summary>
    /// 表示するテキスト
    /// </summary>
    private Queue<string> m_LogText = new Queue<string>();

    /// <summary>
    /// 表示時間計測タイマー
    /// </summary>
    private float m_Timer;
    private bool m_Update = true;

    private static readonly int MAX_LOG = 4;
    private static readonly float LOG_TIME = 5f;

    [Inject]
    public void Construct(IPlayerLoopManager loopManager)
    {
        loopManager.GetUpdateEvent.SubscribeWithState(this, (_, self) => self.OnUpdate()).AddTo(this);
    }

    /// <summary>
    /// ログを表示
    /// </summary>
    /// <param name="log"></param>
    void IBattleLogManager.Log(string log)
    {
        if (log == string.Empty)
            return;

        m_AllTextLog.Add(log);
        m_LogText.Enqueue(log);
        if (m_LogText.Count > MAX_LOG)
            m_LogText.Dequeue();

        var sb = new StringBuilder();
        int count = 0;
        foreach (var t in m_LogText)
        {
            sb.Append(t);
            if (++count != m_LogText.Count)
                sb.Append("\n");
        }

        m_Text.text = sb.ToString();

#if DEBUG
        Debug.Log("バトルログ：" + log);
#endif

        m_Timer = 0f;
        m_BattleLog.SetActive(true);
    }

    private void Deactive()
    {
        m_BattleLog.SetActive(false);
        m_LogText.Clear();
    }
    void IBattleLogManager.Deactive() => Deactive();

    /// <summary>
    /// 時間経過でログ非表示
    /// </summary>
    private void OnUpdate()
    {
        if (m_Update == false)
            return;

        m_Timer += Time.deltaTime;
        if (m_Timer >= LOG_TIME)
            Deactive();
    }

    IDisposable IBattleLogManager.FixLogForUi(string log)
    {
        var sb = new StringBuilder();
        sb.Append(log);
        sb.Append("\n");
        sb.Append("\n");
        sb.Append("Enterキー: 決定");
        sb.Append("\n");
        sb.Append("Qキー: 戻る");

        m_Text.text = sb.ToString();
        m_BattleLog.SetActive(true);
        m_Update = false;

        return Disposable.CreateWithState(this, self =>
        {
            self.m_Update = true;
            self.Deactive();
        });
    }
}