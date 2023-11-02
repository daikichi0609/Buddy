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
    private int m_InGameProgress;
    public int IncrementInGameProgress() => ++m_InGameProgress;

    /// <summary>
    /// 進捗度最大か
    /// </summary>
    public bool IsMaxInGameProgress => m_InGameProgress >= m_IsCompletedIntro.Length - 1;

    /// <summary>
    /// 最終決戦のフレンドインデックス
    /// </summary>
    [SerializeField]
    private int m_FinalBattleFriendIndex;
    public void SetFinalBattleFriendIndex(int index) => m_FinalBattleFriendIndex = index;

    /// <summary>
    /// フレンドなしフラグ
    /// </summary>
    private bool m_NoFriend;
    public void SetNoFriend(bool no) => m_NoFriend = no;

    /// <summary>
    /// 最終決戦前まで物語の進捗度依存
    /// それ以降は専用パラメタ
    /// </summary>
    public int FriendIndex
    {
        get
        {
            if (m_NoFriend == true)
                return -1;
            return IsMaxInGameProgress ? m_FinalBattleFriendIndex : m_InGameProgress;
        }
    }
    public DUNGEON_THEME DungeonTheme => (DUNGEON_THEME)m_InGameProgress;

    /// <summary>
    /// イントロタイムラインタイプ
    /// </summary>
    public TIMELINE_TYPE CurrentIntroTimelineTheme
    {
        get
        {
            if (IsMaxInGameProgress == true)
                return TIMELINE_TYPE.FINAL_INTRO;

            return m_InGameProgress switch
            {
                0 => TIMELINE_TYPE.INTRO,
                1 => TIMELINE_TYPE.BERRY_INTRO,
                2 => TIMELINE_TYPE.DORCHE_INTRO,
                3 => TIMELINE_TYPE.FINAL_INTRO,
                _ => TIMELINE_TYPE.NONE
            };
        }
    }

    /// <summary>
    /// 冒頭イベント終了フラグ
    /// </summary>
    [SerializeField]
    private bool[] m_IsCompletedIntro;
    public bool CurrentIntroCompleted
    {
        get
        {
            if (m_InGameProgress < 0 || m_InGameProgress >= m_IsCompletedIntro.Length)
                return true;
            return m_IsCompletedIntro[m_InGameProgress];
        }
        set => m_IsCompletedIntro[m_InGameProgress] = value;
    }

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