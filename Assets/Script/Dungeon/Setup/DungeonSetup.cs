
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System;

[CreateAssetMenu(menuName = "MyScriptable/Dungeon/Setup")]
[Serializable]
public class DungeonSetup : ScriptableObject
{
    /// <summary>
    /// ダンジョン名
    /// </summary>
    [SerializeField]
    [ResizableTextArea]
    private string m_DungeonName;
    public string DungeonName => m_DungeonName;

    /// <summary>
    /// 階数
    /// </summary>
    [SerializeField]
    [Range(0, 100)]
    private int m_FloorCount;
    public int FloorCount => m_FloorCount;

    /// <summary>
    /// ダンジョンの広さ
    /// </summary>
    [SerializeField]
    private Vector2Int m_MapSize;
    public Vector2Int MapSize => m_MapSize;

    /// <summary>
    /// 部屋の数の範囲
    /// </summary>
    [SerializeField]
    [MinMaxSlider(0, 20)]
    private Vector2Int m_RoomCountRange;
    public int RoomCountMin => m_RoomCountRange.x;
    public int RoomCountMax => m_RoomCountRange.y;

    /// <summary>
    /// 敵の出現テーブル
    /// </summary>
    [SerializeField]
    private EnemyTableSetup m_EnemyTable;
    public EnemyTableSetup EnemyTable => m_EnemyTable;

    /// <summary>
    /// 敵の数の範囲
    /// </summary>
    [SerializeField]
    [MinMaxSlider(0, 20)]
    private Vector2Int m_EnemyCountRange;
    public int EnemyCountMin => m_EnemyCountRange.x;
    public int EnemyCountMax => m_EnemyCountRange.y;

    /// <summary>
    /// モンスターハウス確率
    /// </summary>
    [SerializeField]
    [Range(0f, 1f)]
    private float m_MonsterHouseRate;
    public float MonsterHouseRate => m_MonsterHouseRate;

    /// <summary>
    /// 罠の確率
    /// </summary>
    [SerializeField, Range(0f, 1f)]
    private float m_TrapProb;
    public float TrapProb => m_TrapProb;

    /// <summary>
    /// アイテムの数
    /// </summary>
    [SerializeField, MinMaxSlider(0, 10)]
    private Vector2Int m_ItemCountRange;
    public int ItemCountMin => m_ItemCountRange.x;
    public int ItemCountMax => m_ItemCountRange.y;
}
