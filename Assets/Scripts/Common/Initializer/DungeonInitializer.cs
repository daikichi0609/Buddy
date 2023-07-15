using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System.Threading.Tasks;
using Zenject.SpaceFighter;
using Zenject;

public class DungeonInitializer : SceneInitializer
{
    [Inject]
    private DungeonProgressHolder m_ProgressHolder;
    [Inject]
    private IDungeonProgressManager m_DungeonProgress;
    [Inject]
    private IDungeonDeployer m_DungeonDeployer;
    [Inject]
    private IDungeonContentsDeployer m_DungeonContentsDeployer;
    [Inject]
    private IDungeonCharacterProgressManager m_DungeonCharacterProgressManager;
    [Inject]
    private ITurnManager m_TurnManager;

    /// <summary>
    /// スタート処理
    /// </summary>
    protected override async Task OnStart()
    {
        string where = m_DungeonProgress.CurrentFloor.ToString() + "F";

        var elementSetup = m_ProgressHolder.CurrentDungeonSetup.ElementSetup;
        await m_DungeonDeployer.DeployDungeon(elementSetup);
        await m_DungeonContentsDeployer.DeployAll();

        var bgm = MonoBehaviour.Instantiate(m_ProgressHolder.CurrentDungeonSetup.BGM);
        m_BGMHandler.SetBGM(bgm);

        m_DungeonCharacterProgressManager.AdoptSaveData();

        // 明転
        await m_FadeManager.TurnBright(m_ProgressHolder.CurrentDungeonSetup.DungeonName, where);
        m_TurnManager.NextUnitAct();
    }
}