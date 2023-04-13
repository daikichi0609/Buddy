using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(menuName = "MyScriptable/Trap/Setup")]
public class TrapSetup : ScriptableObject
{
    /// <summary>
    /// プレハブ
    /// </summary>
    [SerializeField]
    private GameObject m_Prefab;
    public GameObject Prefab => m_Prefab;

    /// <summary>
    /// 罠タイプ
    /// </summary>
    [SerializeField]
    private TRAP_TYPE m_Type;
    public TRAP_TYPE Type => m_Type;
}
