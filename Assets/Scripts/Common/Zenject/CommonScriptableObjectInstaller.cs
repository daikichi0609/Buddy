using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using Zenject;

[CreateAssetMenu(menuName = "MyScriptable/Zenject/CommonScriptableObjectInstaller")]
public class CommonScriptableObjectInstaller : ScriptableObjectInstaller<CommonScriptableObjectInstaller>
{
    /// <summary>
    /// インゲーム進行度
    /// </summary>
    [SerializeField]
    [Expandable]
    private InGameProgressHolder m_InGameProgressHolder;

    /// <summary>
    /// ダンジョン進行度
    /// </summary>
    [SerializeField]
    [Expandable]
    private DungeonProgressHolder m_DungeonProgressHolder;

    /// <summary>
    /// ダンジョン途中キャラクター持ち越し情報
    /// </summary>
    [SerializeField]
    [Expandable]
    private DungeonCharacterProgressSaveData m_DungeonCharacterSaveData;

    /// <summary>
    /// ホームセットアップ
    /// </summary>
    [SerializeField]
    [Expandable]
    private HomeSetup m_HomeSetup;

    /// <summary>
    /// キャラクター情報
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

    /// <summary>
    /// エフェクトデータ
    /// </summary>
    [SerializeField]
    [Expandable]
    private EffectSetup m_EffectSetup;

    /// <summary>
    /// サウンドデータ
    /// </summary>
    [SerializeField]
    [Expandable]
    private SoundSetup m_SoundSetup;

    public override void InstallBindings()
    {
        Container.BindInstance(m_InGameProgressHolder).AsSingle();
        Container.BindInstance(m_HomeSetup).AsSingle();
        Container.BindInstance(m_DungeonProgressHolder).AsSingle();
        Container.BindInstance(m_CurrentCharacterHolder).AsSingle();
        Container.BindInstance(m_DungeonCharacterSaveData).AsSingle();
        Container.BindInstance(m_MasterData.CharacterMasterSetup).AsSingle();
        Container.BindInstance(m_EffectSetup).AsSingle();
        Container.BindInstance(m_SoundSetup).AsSingle();
    }
}