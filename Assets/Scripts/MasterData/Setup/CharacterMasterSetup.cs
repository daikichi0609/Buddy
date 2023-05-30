using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

[CreateAssetMenu(menuName = "MyScriptable/Master/Character")]
[Serializable]
public class CharacterMasterSetup : ScriptableObject
{
    [SerializeField]
    [BoxGroup("サウンド")]
    private GameObject m_AttackSound;
    public GameObject AttackSound => m_AttackSound;

    [SerializeField]
    [BoxGroup("サウンド")]
    private GameObject m_DamageSound;
    public GameObject DamageSound => m_DamageSound;

    [SerializeField]
    [BoxGroup("サウンド")]
    private GameObject m_MissSound;
    public GameObject MissSound => m_MissSound;
}
