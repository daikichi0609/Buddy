using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using UniRx;
using System;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using UnityEngine.Analytics;
using Zenject;

public interface IDungeonCharacterProgressManager
{
    /// <summary>
    /// データセーブ
    /// </summary>
    void WriteSaveData();

    /// <summary>
    /// データ適用
    /// </summary>
    void AdoptSaveData();

    /// <summary>
    /// データリセット
    /// </summary>
    void ResetAll();
}

public class DungeonCharacterProgressManager : IDungeonCharacterProgressManager
{
    [Inject]
    private DungeonCharacterProgressSaveData m_DungeonCharacterSaveData;
    [Inject]
    private ITeamLevelHandler m_TeamLevelHandler;
    [Inject]
    private ITeamInventory m_TeamInventory;

    /// <summary>
    /// キャラの状態をセーブ
    /// </summary>
    void IDungeonCharacterProgressManager.WriteSaveData()
    {
        var exp = m_TeamLevelHandler.Exp;
        var items = m_TeamInventory.Items;
        m_DungeonCharacterSaveData.WriteData(exp, items);
    }

    /// <summary>
    /// データ適用
    /// </summary>
    void IDungeonCharacterProgressManager.AdoptSaveData()
    {
        m_TeamLevelHandler.SetExp(m_DungeonCharacterSaveData.Exp);
        m_TeamInventory.SetItems(m_DungeonCharacterSaveData.Items);
    }

    /// <summary>
    /// リセット
    /// </summary>
    void IDungeonCharacterProgressManager.ResetAll() => m_DungeonCharacterSaveData.ResetData();
}
