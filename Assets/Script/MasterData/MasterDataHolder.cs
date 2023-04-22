using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMasterDataHolder : ISingleton
{
    CharacterMasterSetup CharacterMasterSetup { get; }
}

public class MasterDataHolder : Singleton<MasterDataHolder, IMasterDataHolder>, IMasterDataHolder
{
    [SerializeField]
    private CharacterMasterSetup m_CharacterMasterSetup;
    CharacterMasterSetup IMasterDataHolder.CharacterMasterSetup => m_CharacterMasterSetup;
}
