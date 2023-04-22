using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "MyScriptable/Master/Character")]
[Serializable]
public class CharacterMasterSetup : ScriptableObject
{
    [SerializeField]
    private GameObject m_AttackSound;
    public GameObject AttackSound => m_AttackSound;

    [SerializeField]
    private GameObject m_DamageSound;
    public GameObject DamageSound => m_DamageSound;

    [SerializeField]
    private GameObject m_MissSound;
    public GameObject MissSound => m_MissSound;
}
