using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System;

[CreateAssetMenu(menuName = "MyScriptable/Dungeon/ElementSetup")]
[Serializable]
public class DungeonElementSetup : ScriptableObject
{
    /// <summary>
    /// 部屋
    /// </summary>
    [SerializeField, Header("部屋セル")]
    private GameObject m_Room;
    public GameObject Room => m_Room;

    /// <summary>
    /// 通路
    /// </summary>
    [SerializeField, Header("通路セル")]
    private GameObject m_Path;
    public GameObject Path => m_Path;

    /// <summary>
    /// 壁
    /// </summary>
    [SerializeField, Header("壁セル")]
    private GameObject m_Wall;
    public GameObject Wall => m_Wall;

    /// <summary>
    /// 階段
    /// </summary>
    [SerializeField, Header("階段セル")]
    private GameObject m_Stairs;
    public GameObject Stairs => m_Stairs;
}
