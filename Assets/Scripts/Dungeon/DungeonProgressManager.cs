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
    /// 現在の階層
    /// </summary>
    int CurrentFloor { get; }
    IObservable<int> FloorChanged { get; }

    /// <summary>
    /// 次の階層へ
    /// </summary>
    Task NextFloor();

    /// <summary>
    /// ゲーム終了
    /// </summary>
    /// <param name="reason">理由</param>
    Task FinishDungeon(FINISH_REASON reason);
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

public class DungeonProgressManager : IDungeonProgressManager
{
    [Inject]
    private DungeonProgressHolder m_ProgressHolder;
    [Inject]
    private IFadeManager m_FadeManager;
    [Inject]
    private IDungeonDeployer m_DungeonDeployer;
    [Inject]
    private IDungeonContentsDeployer m_DungeonContentsDeployer;
    [Inject]
    private ITurnManager m_TurnManager;
    [Inject]
    private IYesorNoQuestionUiManager m_QuestionManager;
    [Inject]
    private IDungeonCharacterProgressManager m_DungeonCharacterProgressManager;
    [Inject]
    private ISoundHolder m_SoundHolder;

    private static readonly string STAIRS = "Stairs";

    /// <summary>
    /// 現在の階層
    /// </summary>
    private ReactiveProperty<int> m_CurrentFloor = new ReactiveProperty<int>(1);
    int IDungeonProgressManager.CurrentFloor => m_CurrentFloor.Value;
    IObservable<int> IDungeonProgressManager.FloorChanged => m_CurrentFloor;

    /// <summary>
    /// 次の階
    /// </summary>
    /// <returns></returns>
    async Task IDungeonProgressManager.NextFloor()
    {
        m_TurnManager.StopUnitAct(); // ユニット停止
        m_QuestionManager.Deactivate(); // UI非表示
        if (m_SoundHolder.TryGetSound(STAIRS, out var sound) == true) // 音
            sound.Play();

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
        await m_FadeManager.StartFade(this, async self => await self.RebuildDungeon(), m_ProgressHolder.CurrentDungeonSetup.DungeonName, where);
        m_TurnManager.NextUnitAct();
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

        m_DungeonCharacterProgressManager.WriteSaveData();
        await m_FadeManager.LoadScene(sceneName);
    }

    /// <summary>
    /// イベントによるシーン移動
    /// </summary>
    /// <param name="reason"></param>
    async Task IDungeonProgressManager.FinishDungeon(FINISH_REASON reason)
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
