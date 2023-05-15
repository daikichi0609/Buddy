using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using UniRx;
using System;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

public interface IDungeonProgressManager : ISingleton
{
    /// <summary>
    /// 進行度に応じた現在のダンジョン設定
    /// </summary>
    DungeonSetup CurrentDungeonSetup { get; }

    /// <summary>
    /// ボスバトル設定
    /// </summary>
    BossBattleSetup CurrentBossBattleSetup { get; }

    /// <summary>
    /// 現在のダンジョンテーマ
    /// </summary>
    DUNGEON_THEME CurrentDungeonTheme { set; }

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

    /// <summary>
    /// ダンジョン初期化
    /// </summary>
    void InitializeDungeon();

    /// <summary>
    /// 次の階層へ
    /// </summary>
    Task NextFloor();
    IObservable<int> FloorChanged { get; }
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
    BossBattleSetup IDungeonProgressManager.CurrentBossBattleSetup => CurrentDungeonSetupHolder.BossBattleSetup;

    /// <summary>
    /// 現在のダンジョンテーマ
    /// </summary>
    [ShowNonSerializedField]
    private DUNGEON_THEME m_CurrentDungeonTheme;
    DUNGEON_THEME IDungeonProgressManager.CurrentDungeonTheme { set => m_CurrentDungeonTheme = value; }

    /// <summary>
    /// 現在のダンジョン進行度
    /// </summary>
    // [ShowNonSerializedField]
    [SerializeField]
    private int m_CurrentProgress;

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
    /// ダンジョン初期化
    /// </summary>
    void IDungeonProgressManager.InitializeDungeon()
    {
        string where = m_CurrentFloor.Value.ToString() + "F";
        // 明転
        FadeManager.Interface.TurnBright(CurrentDungeonSetup.DungeonName, where);

        var elementSetup = DungeonProgressManager.Interface.CurrentDungeonSetup.ElementSetup;
        DungeonDeployer.Interface.DeployDungeon(elementSetup);
        DungeonContentsDeployer.Interface.DeployAll();

        var bgm = Instantiate(CurrentDungeonSetup.BGM);
        BGMHandler.Interface.SetBGM(bgm);
    }

    /// <summary>
    /// 次の階
    /// </summary>
    /// <returns></returns>
    async Task IDungeonProgressManager.NextFloor()
    {
        YesorNoQuestionUiManager.Interface.Deactive();

        int maxFloor = CurrentDungeonSetup.FloorCount;
        // すでに最上階にいるならチェックポイントへ
        if (m_CurrentFloor.Value >= maxFloor)
        {
            await ToCheckPoint();
            return;
        }

        // 階層up
        m_CurrentFloor.Value++;

        // 暗転 & ダンジョン再構築
        string where = m_CurrentFloor.Value.ToString() + "F";
        await FadeManager.Interface.StartFade(() => RebuildDungeon(), CurrentDungeonSetup.DungeonName, where);
    }

    /// <summary>
    /// ダンジョン再構築
    /// </summary>
    private void RebuildDungeon()
    {
        // ダンジョン撤去
        DungeonDeployer.Interface.RemoveDungeon();
        DungeonContentsDeployer.Interface.RemoveAll();

        // ダンジョン再構築
        var setup = DungeonProgressManager.Interface.CurrentDungeonSetup.ElementSetup;
        DungeonDeployer.Interface.DeployDungeon(setup);
        DungeonContentsDeployer.Interface.DeployAll();
    }

    /// <summary>
    /// チェックポイントへ
    /// </summary>
    /// <returns></returns>
    async private Task ToCheckPoint()
    {
        m_CurrentFloor.Value = 1;
        int maxProgress = CurrentDungeonSetupHolder.DungeonSetup.Length;
        // 最終進行度ならボスバトルに移動
        string sceneName = ++m_CurrentProgress < maxProgress ? SceneName.SCENE_CHECKPOINT : SceneName.SCENE_BOSS_BATTLE;

        await FadeManager.Interface.LoadScene(sceneName);
    }
}
