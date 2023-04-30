using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System;

[CreateAssetMenu(menuName = "MyScriptable/Character/CharacterSetup")]
[Serializable]
public class CharacterSetup : PrefabSetup
{
    /// <summary>
    /// ステータス
    /// </summary>
    [SerializeField, Header("ステータス")]
    [Expandable]
    private BattleStatus m_Status;
    public BattleStatus Status => m_Status;

    /// <summary>
    /// アウトゲーム用Prefab
    /// </summary>
    [SerializeField, Header("アウトゲーム用Prefab")]
    private GameObject m_OutGamePrefab;
    public GameObject OutGamePrefab => m_OutGamePrefab;
}
