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
    public bool IsMaxProgress => m_Progress >= m_IsCompletedIntro.Length - 1;

    /// <summary>
    /// 冒頭イベント終了フラグ
    /// </summary>
    [SerializeField]
    private bool[] m_IsCompletedIntro;
    public bool[] IsCompletedIntro => m_IsCompletedIntro;
    public bool CurrentCompletedIntro { set => m_IsCompletedIntro[m_Progress] = value; }

    /// <summary>
    /// 負けて前回の地点まで戻るフラグ
    /// </summary>
    [SerializeField]
    private bool m_LoseBack;
    public bool LoseBack { get => m_LoseBack; set => m_LoseBack = value; }

    /// <summary>
    /// ボス撃破フラグ
    /// </summary>
    [SerializeField]
    private bool m_DefeatBoss;
    public bool DefeatBoss { get => m_DefeatBoss; set => m_DefeatBoss = value; }
}