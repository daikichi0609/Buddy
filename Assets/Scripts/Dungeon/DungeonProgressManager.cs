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

public interface IDungeonProgressManager
{
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
    /// 次の階層へ
    /// </summary>
    Task NextFloor();

    /// <summary>
    /// 現在の階層
    /// </summary>
    int CurrentFloor { get; }
    IObservable<int> FloorChanged { get; }

    /// <summary>
    /// ゲーム終了
    /// </summary>
    /// <param name="reason">理由</param>
    void FinishDungeon(FINISH_REASON reason);
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

/// <summary>
/// ゲーム終了理由
/// </summary>
public enum FINISH_REASON
{
    PLAYER_DEAD,
    BOSS_DEAD,
}

public class DungeonProgressManager : IDungeonProgressManager, IInitializable
{
    [Inject]
    private DungeonProgressHolder m_ProgressHolder;
    [Inject]
    private IFadeManager m_FadeManager;
    [Inject]
    private IBGMHandler m_BGMHandler;
    [Inject]
    private IDungeonDeployer m_DungeonDeployer;
    [Inject]
    private IDungeonContentsDeployer m_DungeonContentsDeployer;
    [Inject]
    private ITurnManager m_TurnManager;
    [Inject]
    private IYesorNoQuestionUiManager m_QuestionManager;

    /// <summary>
    /// 現在の階層
    /// </summary>
    private ReactiveProperty<int> m_CurrentFloor = new ReactiveProperty<int>(1);
    int IDungeonProgressManager.CurrentFloor => m_CurrentFloor.Value;
    IObservable<int> IDungeonProgressManager.FloorChanged => m_CurrentFloor;

    async void IInitializable.Initialize() => await InitializeDungeon();

    /// <summary>
    /// ダンジョン初期化
    /// </summary>
    private async Task InitializeDungeon()
    {
        string where = m_CurrentFloor.Value.ToString() + "F";

        var elementSetup = m_ProgressHolder.CurrentDungeonSetup.ElementSetup;
        await m_DungeonDeployer.DeployDungeon(elementSetup);
        await m_DungeonContentsDeployer.DeployAll();

        var bgm = MonoBehaviour.Instantiate(m_ProgressHolder.CurrentDungeonSetup.BGM);
        m_BGMHandler.SetBGM(bgm);

        // 明転
        await m_FadeManager.TurnBright(m_ProgressHolder.CurrentDungeonSetup.DungeonName, where);
    }

    /// <summary>
    /// ランダムな敵キャラセットアップを重み抽選
    /// </summary>
    /// <returns></returns>
    CharacterSetup IDungeonProgressManager.GetRandomEnemySetup()
    {
        var t = m_ProgressHolder.CurrentDungeonSetup.EnemyTable;
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
        var d = m_ProgressHolder.CurrentDungeonSetupHolder.ItemDeploySetup;
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
        var d = m_ProgressHolder.CurrentDungeonSetupHolder.TrapDeploySetup;
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
        // ユニット停止
        m_TurnManager.StopUnitAct();

        // UI非表示
        m_QuestionManager.Deactive();

        int maxFloor = m_ProgressHolder.CurrentDungeonSetup.FloorCount;
        // すでに最上階にいるならチェックポイントへ
        if (m_CurrentFloor.Value >= maxFloor)
        {
            // 進行度+1
            m_ProgressHolder.CurrentProgress++;
            await MoveScene();
            return;
        }

        // 階層up
        m_CurrentFloor.Value++;

        // 暗転 & ダンジョン再構築
        string where = m_CurrentFloor.Value.ToString() + "F";
        await m_FadeManager.StartFade(async () => await RebuildDungeon(), m_ProgressHolder.CurrentDungeonSetup.DungeonName, where);
    }

    /// <summary>
    /// ダンジョン再構築
    /// </summary>
    private async Task RebuildDungeon()
    {
        // ダンジョン撤去
        m_DungeonDeployer.RemoveDungeon();
        m_DungeonContentsDeployer.RemoveAll();

        // ダンジョン再構築
        var setup = m_ProgressHolder.CurrentDungeonSetup.ElementSetup;
        await m_DungeonDeployer.DeployDungeon(setup);
        await m_DungeonContentsDeployer.DeployAll();
    }

    /// <summary>
    /// 進行度に合わせたシーン移動
    /// </summary>
    /// <returns></returns>
    async private Task MoveScene()
    {
        m_CurrentFloor.Value = 1;
        // シーン名
        string sceneName = "";
        if (m_ProgressHolder.CurrentProgress == 0)
            sceneName = SceneName.SCENE_HOME;
        else if (m_ProgressHolder.CurrentProgress == m_ProgressHolder.MaxProgress)
            sceneName = SceneName.SCENE_BOSS_BATTLE;
        else
            sceneName = SceneName.SCENE_CHECKPOINT;

        await m_FadeManager.LoadScene(sceneName);
    }

    /// <summary>
    /// イベントによるシーン移動
    /// </summary>
    /// <param name="reason"></param>
    async void IDungeonProgressManager.FinishDungeon(FINISH_REASON reason)
    {
        m_TurnManager.StopUnitAct();

        if (reason == FINISH_REASON.PLAYER_DEAD)
            await MoveScene();
        else if (reason == FINISH_REASON.BOSS_DEAD)
        {
            m_ProgressHolder.CurrentProgress = 0;
            await m_FadeManager.LoadScene(SceneName.SCENE_HOME);
        }
    }
}
