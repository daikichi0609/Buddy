using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using System.Linq;

public interface IMasterDataManager : ISingleton
{

}

public class MasterDataManager : Singleton<MasterDataManager, ISampleManager>, ISampleManager
{
    [SerializeField, EnumIndex(typeof(DUNGEON_THEME))]
    private DungeonSetupHolder[] m_DungeonSetupHolder;
}
