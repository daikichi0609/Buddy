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
    /// エレメント設定
    /// </summary>
    [SerializeField, Expandable]
    [Header("構築物設定")]
    private DungeonElementSetup m_ElementSetup;
    public DungeonElementSetup ElementSetup => m_ElementSetup;

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
}