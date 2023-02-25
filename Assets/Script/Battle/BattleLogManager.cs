using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public interface IBattleLogManager : ISingleton
{
    void Log(string log);
}

public class BattleLogManager : Singleton<BattleLogManager, IBattleLogManager>, IBattleLogManager
{
    [SerializeField]
    private Text m_Text;

    [SerializeField]
    private GameObject m_BattleLog;

    /// <summary>
    /// 表示するテキスト
    /// </summary>
    private Queue<string> m_LogText = new Queue<string>();

    /// <summary>
    /// 表示時間計測タイマー
    /// </summary>
    private float m_Timer;

    private static readonly int MAX_LOG = 4;

    private static readonly float LOG_TIME = 5f;

    protected override void Awake()
    {
        base.Awake();

        GameManager.Interface.GetUpdateEvent.Subscribe(_ => OnUpdate());
    }

    /// <summary>
    /// ログを表示
    /// </summary>
    /// <param name="log"></param>
    void IBattleLogManager.Log(string log)
    {
        m_LogText.Enqueue(log);
        if (m_LogText.Count > MAX_LOG)
            m_LogText.Dequeue();

        var sb = new StringBuilder();
        foreach (var t in m_LogText)
        {
            sb.Append(t);
            sb.Append("\n");
        }

        m_Text.text = sb.ToString();
        Debug.Log("バトルログ：" + log);

        m_Timer = 0f;
        m_BattleLog.SetActive(true);
    }

    /// <summary>
    /// 時間経過でログ非表示
    /// </summary>
    private void OnUpdate()
    {
        m_Timer += Time.deltaTime;
        if (m_Timer >= LOG_TIME)
        {
            m_BattleLog.SetActive(false);
            m_LogText.Clear();
        }
    }
}