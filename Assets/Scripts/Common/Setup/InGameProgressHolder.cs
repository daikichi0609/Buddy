using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(menuName = "MyScriptable/Progress")]
public class InGameProgressHolder : ScriptableObject
{
    /// <summary>
    /// 進捗度
    /// </summary>
    [SerializeField]
    private int m_Progress;
    public int Progress { get => m_Progress; set => m_Progress = value; }

    /// <summary>
    /// 冒頭イベント終了フラグ
    /// </summary>
    [SerializeField]
    private bool[] m_IsCompletedIntro;
    public bool[] IsCompletedIntro => m_IsCompletedIntro;
    public bool CurrentCompletedIntro { set => m_IsCompletedIntro[m_Progress] = value; }
}