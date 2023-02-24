using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public interface IBattleLogManager : ISingleton
{
    void Log(string log);
}

public class BattleLogManager : Singleton<BattleLogManager, IBattleLogManager>, IBattleLogManager
{
    [SerializeField]
    private Text m_Text;

    private Queue<string> m_LogText = new Queue<string>();

    private static readonly int MAX_LOG = 3;

    void IBattleLogManager.Log(string log)
    {
        m_LogText.Enqueue(log);
        if (m_LogText.Count > MAX_LOG)
            m_LogText.Dequeue();

        var sb = new StringBuilder();
        foreach(var t in m_LogText)
        {
            sb.Append(t);
        }

        m_Text.text = sb.ToString();
        Debug.Log("バトルログ：" + log);
    }
}