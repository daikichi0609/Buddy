using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using UniRx;
using System;
using System.Threading.Tasks;

public interface IDungeonProgressManager : ISingleton
{
    /// <summary>
    /// 進行度に応じた現在のダンジョン設定
    /// </summary>
    DungeonSetup CurrentDungeonSetup { get; }

    /// <summary>
    /// ダンジョンエレメント設定
    /// </summary>
    DungeonElementSetup CurrentElementSetup { get; }

    /// <summary>
    /// 現在のダンジョンテーマ
    /// </summary>
    DUNGEON_THEME CurrentDungeonTheme { set; }

    /// <summary>
    /// 現在のダンジョン進行度
    /// </summary>
    int CurrentProgress { set; }

    /// <summary>
    /// 次の階層へ
    /// </summary>
    Task NextFloor();
    IObservable<int> FloorChanged { get; }

    /// <summary>
    /// ランダムな敵キャラクターセットアップを取得
    /// </summary>
    /// <returns></returns>
    CharacterSetup GetRandomEnemySetup();

    /// <summary>
    /// ランダムなアイテムセットアップを取得
    /// </summary>
    /// <returns></returns>
    ItemSetup GetRandomItemSetup();

    /// <summary>
    /// ランダムな罠セットアップを取得
    /// </summary>
    /// <returns></returns>
    TrapSetup GetRandomTrapSetup();
}

/// <summary>
/// ダンジョン種類
/// </summary>
public enum DUNGEON_THEME
{
    GRASS = 0,
    ROCK = 1,
    CRYSTAL = 2,
    WHITE = 3,
}

public class DungeonProgressManager : Singleton<DungeonProgressManager, IDungeonProgressManager>, IDungeonProgressManager
{
    /// <summary>
    /// ダンジョンセットアップ集
    /// </summary>
    [SerializeField]
    private DungeonSetupHolder[] m_DungeonSetupHolders = new DungeonSetupHolder[0];
    private DungeonSetupHolder CurrentDungeonSetupHolder => m_DungeonSetupHolders[(int)m_CurrentDungeonTheme];
    public DungeonSetup CurrentDungeonSetup => CurrentDungeonSetupHolder.DungeonSetup[m_CurrentProgress];
    DungeonElementSetup IDungeonProgressManager.CurrentElementSetup => CurrentDungeonSetupHolder.ElementSetup;

    /// <summary>
    /// 現在のダンジョンテーマ
    /// </summary>
    [ShowNonSerializedField]
    private DUNGEON_THEME m_CurrentDungeonTheme;
    DUNGEON_THEME IDungeonProgressManager.CurrentDungeonTheme { set => m_CurrentDungeonTheme = value; }

    /// <summary>
    /// 現在のダンジョン進行度
    /// </summary>
    [ShowNonSerializedField]
    private int m_CurrentProgress;
    int IDungeonProgressManager.CurrentProgress { set => m_CurrentProgress = value; }

    /// <summary>
    /// 現在の階層
    /// </summary>
    private ReactiveProperty<int> m_CurrentFloor = new ReactiveProperty<int>(1);
    IObservable<int> IDungeonProgressManager.FloorChanged => m_CurrentFloor;

    /// <summary>
    /// ランダムな敵キャラセットアップを重み抽選
    /// </summary>
    /// <returns></returns>
    CharacterSetup IDungeonProgressManager.GetRandomEnemySetup()
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
    ItemSetup IDungeonProgressManager.GetRandomItemSetup()
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
    TrapSetup IDungeonProgressManager.GetRandomTrapSetup()
    {
        var d = CurrentDungeonSetupHolder.TrapDeploySetup;
        int length = d.TrapPacks.Length;
        int[] weights = new int[length];

        for (int i = 0; i < length; i++)
            weights[i] = d.TrapPacks[i].Weight;

        var index = WeightedRandomSelector.SelectIndex(weights);
        return d.TrapPacks[index].Setup;
    }

    /// <summary>
    /// 次の階
    /// </summary>
    /// <returns></returns>
    async Task IDungeonProgressManager.NextFloor()
    {
        YesorNoQuestionUiManager.Interface.Deactive();

        // 階層up
        m_CurrentFloor.Value++;

        // 暗転 & ダンジョン再構築
        await FadeManager.Interface.NextFloor(() => RebuildDungeon(), m_CurrentFloor.Value, CurrentDungeonSetup.DungeonName);

        // 行動許可
        TurnManager.Interface.AllCharaActionable();
    }

    /// <summary>
    /// ダンジョン再構築
    /// </summary>
    private void RebuildDungeon()
    {
        // ダンジョン撤去
        DungeonManager.Interface.RemoveDungeon();
        DungeonContentsDeployer.Interface.Remove();

        // ダンジョン再構築
        DungeonManager.Interface.DeployDungeon();
        DungeonContentsDeployer.Interface.Deploy();
    }
}
