using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

[CreateAssetMenu(menuName = "MyScriptable/Master/Character")]
[Serializable]
public class CharacterMasterSetup : ScriptableObject
{
    // ----- レベル ----- //

    [SerializeField, Header("初めのレベルアップボーダー")]
    [BoxGroup("レベル")]
    private int m_LevelUpFirstBorder;
    public int LevelUpFirstBorder => m_LevelUpFirstBorder;

    [SerializeField, Header("次レベルに必要な経験値倍率")]
    [BoxGroup("レベル")]
    private float m_NextExMag;
    public float NextExMag => m_NextExMag;

    [SerializeField, Header("最大レベル")]
    [BoxGroup("レベル")]
    private int m_MaxLevel;
    public int MaxLevel => m_MaxLevel;
}
