using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(menuName = "MyScriptable/Master/MasterData")]
public class MasterDataHolder : ScriptableObject
{
    [SerializeField]
    [Expandable]
    private CharacterMasterSetup m_CharacterMasterSetup;
    public CharacterMasterSetup CharacterMasterSetup => m_CharacterMasterSetup;
}
