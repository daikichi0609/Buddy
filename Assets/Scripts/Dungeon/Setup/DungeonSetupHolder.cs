using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System;

[CreateAssetMenu(menuName = "MyScriptable/Dungeon/SetupHolder")]
public class DungeonSetupHolder : ScriptableObject
{
    /// <summary>
    /// ダンジョンの基本設定
    /// </summary>
    [SerializeField, ReorderableList, Expandable]
    [Header("基本設定")]
    private DungeonSetup[] m_DungeonSetup;
    public DungeonSetup[] DungeonSetup => m_DungeonSetup;

    /// <summary>
    /// 罠設定
    /// </summary>
    [SerializeField, Expandable]
    [Header("罠設定")]
    private TrapDeploySetup m_TrapDeploySetup;
    public TrapDeploySetup TrapDeploySetup => m_TrapDeploySetup;

    /// <summary>
    /// アイテム設定
    /// </summary>
    [SerializeField, Expandable]
    [Header("アイテム設定")]
    private ItemDeploySetup m_ItemDeploySetup;
    public ItemDeploySetup ItemDeploySetup => m_ItemDeploySetup;

    /// <summary>
    /// ボスバトル設定
    /// </summary>
    [SerializeField, Expandable]
    [Header("ボスバトル設定")]
    private BossBattleSetup m_BossBattleSetup;
    public BossBattleSetup BossBattleSetup => m_BossBattleSetup;
}