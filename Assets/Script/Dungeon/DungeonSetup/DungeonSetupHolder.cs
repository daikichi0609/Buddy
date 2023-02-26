using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System;

[CreateAssetMenu(menuName = "MyScriptable/Dungeon/SetupHolder")]
public class DungeonSetupHolder : ScriptableObject
{
    [SerializeField]
    private DungeonSetup[] m_Holder;
    public DungeonSetup[] Holder => m_Holder;
}