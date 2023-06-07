using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using Zenject;

[CreateAssetMenu(menuName = "MyScriptable/Zenject/CommonScriptableObjectInstaller")]
public class CommonScriptableObjectInstaller : ScriptableObjectInstaller<CommonScriptableObjectInstaller>
{
    /// <summary>
    /// ホームセットアップ
    /// </summary>
    [SerializeField]
    [Expandable]
    private HomeSetup m_HomeSetup;

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
    private CurrentCharacterHolder m_CurrentCharacterHolder;

    /// <summary>
    /// マスターデータ
    /// </summary>
    [SerializeField]
    [Expandable]
    private MasterDataHolder m_MasterData;

    public override void InstallBindings()
    {
        Container.BindInstance(m_HomeSetup).AsSingle();
        Container.BindInstance(m_ProgressHolder).AsSingle();
        Container.BindInstance(m_CurrentCharacterHolder).AsSingle();
        Container.BindInstance(m_MasterData).AsSingle();
    }
}