using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using Zenject;

[CreateAssetMenu(menuName = "MyScriptable/Zenject/CommonScriptableObjectInstaller")]
public class CommonScriptableObjectInstaller : ScriptableObjectInstaller<CommonScriptableObjectInstaller>
{
    /// <summary>
    /// ダンジョン進行度
    /// </summary>
    [SerializeField]
    [Expandable]
    private DungeonProgressHolder m_ProgressHolder;

    /// <summary>
    /// アウトゲーム情報
    /// </summary>
    [SerializeField]
    [Expandable]
    private OutGameInfoHolder m_OutGameInfoHolder;

    /// <summary>
    /// マスターデータ
    /// </summary>
    [SerializeField]
    [Expandable]
    private MasterDataHolder m_MasterData;

    public override void InstallBindings()
    {
        Container.BindInstance(m_ProgressHolder).AsSingle();

        Container.BindInstance(m_OutGameInfoHolder).AsSingle();

        Container.BindInstance(m_MasterData).AsSingle();
    }
}