using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System;

[CreateAssetMenu(menuName = "MyScriptable/Character/CharacterSetup")]
[Serializable]
public class CharacterSetup : ScriptableObject
{
    /// <summary>
    /// プレハブ
    /// </summary>
    [SerializeField, Header("プレハブ")]
    private GameObject m_Prefab;
    public GameObject Prefab => m_Prefab;

    /// <summary>
    /// ステータス
    /// </summary>
    [SerializeField, Expandable]
    private BattleStatus m_Status;
    public BattleStatus Status => m_Status;
}
