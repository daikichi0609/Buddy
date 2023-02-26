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

    /// <summary>
    /// 階数
    /// </summary>
    [SerializeField]
    [Range(0, 100)]
    private int m_FloorCount;

    /// <summary>
    /// 部屋の数最大
    /// </summary>
    [SerializeField]
    [Range(0, 15)]
    private int m_MaxRoomCount;

    /// <summary>
    /// 敵の数最大
    /// </summary>
    [SerializeField]
    [Range(0, 20)]
    private int m_MaxEnemyCount;

    /// <summary>
    /// モンスターハウス確率
    /// </summary>
    [SerializeField]
    [Range(0f, 1f)]
    private float m_MonsterHouseRate;
}
