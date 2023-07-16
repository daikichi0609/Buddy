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

[CreateAssetMenu(menuName = "MyScriptable/Dungeon/Progress")]
public class DungeonProgressHolder : ScriptableObject
{
    /// <summary>
    /// ダンジョンセットアップ集
    /// </summary>
    [SerializeField]
    private DungeonSetupHolder[] m_DungeonSetupHolders = new DungeonSetupHolder[0];
    private DungeonSetupHolder CurrentDungeonSetupHolder => m_DungeonSetupHolders[(int)m_CurrentDungeonTheme];
    public DungeonSetup CurrentDungeonSetup => CurrentDungeonSetupHolder.DungeonSetup[m_CurrentProgress];
    public BossBattleSetup CurrentBossBattleSetup => CurrentDungeonSetupHolder.BossBattleSetup;

    /// <summary>
    /// 現在のダンジョンテーマ
    /// </summary>
    [SerializeField]
    private DUNGEON_THEME m_CurrentDungeonTheme;
    public DUNGEON_THEME CurrentDungeonTheme { get => m_CurrentDungeonTheme; set => m_CurrentDungeonTheme = value; }

    /// <summary>
    /// 現在のダンジョン進行度
    /// </summary>
    // [ShowNonSerializedField]
    [SerializeField]
    private int m_CurrentProgress;
    public int CurrentProgress { get => m_CurrentProgress; set => m_CurrentProgress = value; }
    public int MaxProgress => CurrentDungeonSetupHolder.DungeonSetup.Length;

    /// <summary>
    /// ランダムな敵キャラセットアップを重み抽選
    /// </summary>
    /// <returns></returns>
    public CharacterSetup GetRandomEnemySetup()
    {
        var t = CurrentDungeonSetup.EnemyTable;
        int length = t.EnemyPacks.Length;
        int[] weights = new int[length];

        for (int i = 0; i < length; i++)
            weights[i] = t.EnemyPacks[i].Weight;

        var index = WeightedRandomSelector.SelectIndex(weights);
        return t.EnemyPacks[index].Setup;
    }

    /// <summary>
    /// アイテムセットアップを重み抽選
    /// </summary>
    /// <returns></returns>
    public ItemSetup GetRandomItemSetup()
    {
        var d = CurrentDungeonSetupHolder.ItemDeploySetup;
        int length = d.ItemPacks.Length;
        int[] weights = new int[length];

        for (int i = 0; i < length; i++)
            weights[i] = d.ItemPacks[i].Weight;

        var index = WeightedRandomSelector.SelectIndex(weights);
        return d.ItemPacks[index].Setup;
    }

    /// <summary>
    /// 罠セットアップを重み抽選
    /// </summary>
    /// <returns></returns>
    public TrapSetup GetRandomTrapSetup()
    {
        var d = CurrentDungeonSetupHolder.TrapDeploySetup;
        int length = d.TrapPacks.Length;
        int[] weights = new int[length];

        for (int i = 0; i < length; i++)
            weights[i] = d.TrapPacks[i].Weight;

        var index = WeightedRandomSelector.SelectIndex(weights);
        return d.TrapPacks[index].Setup;
    }
}
