using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using UniRx;
using System;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using UnityEngine.Analytics;
using Zenject;

[CreateAssetMenu(menuName = "MyScriptable/Dungeon/Progress")]
public class DungeonProgressHolder : ScriptableObject
{
    /// <summary>
    /// ダンジョンセットアップ集
    /// </summary>
    [SerializeField]
    private DungeonSetupHolder[] m_DungeonSetupHolders = new DungeonSetupHolder[0];
    public DungeonSetupHolder CurrentDungeonSetupHolder => m_DungeonSetupHolders[(int)m_CurrentDungeonTheme];
    public DungeonSetup CurrentDungeonSetup => CurrentDungeonSetupHolder.DungeonSetup[m_CurrentProgress];
    public BossBattleSetup CurrentBossBattleSetup => CurrentDungeonSetupHolder.BossBattleSetup;

    /// <summary>
    /// 現在のダンジョンテーマ
    /// </summary>
    [SerializeField]
    private DUNGEON_THEME m_CurrentDungeonTheme;
    public DUNGEON_THEME CurrentDungeonTheme { set => m_CurrentDungeonTheme = value; }

    /// <summary>
    /// 現在のダンジョン進行度
    /// </summary>
    // [ShowNonSerializedField]
    [SerializeField]
    private int m_CurrentProgress;
    public int CurrentProgress { get => m_CurrentProgress; set => m_CurrentProgress = value; }
    public int MaxProgress => m_DungeonSetupHolders.Length;
}
