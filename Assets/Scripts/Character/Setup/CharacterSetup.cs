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
    /// スキル
    /// </summary>
    [SerializeField, Header("スキル")]
    [Expandable]
    private SkillSetup m_SkillSetup;
    public SkillSetup SkillSetup => m_SkillSetup;

    /// <summary>
    /// 賢さ
    /// </summary>
    [SerializeField, Header("賢さ")]
    [Expandable]
    private ClevernessSetup m_ClevernessSetup;
    public ClevernessSetup ClevernesssSetup => m_ClevernessSetup;
}
