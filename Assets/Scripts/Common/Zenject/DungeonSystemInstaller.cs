using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using Zenject;

public class DungeonSystemInstaller : MonoInstaller
{
    [SerializeField]
    private bool m_Initialize;

    [SerializeField]
    private GameObject m_DungeonUiSystem;

    public override void InstallBindings()
    {
        // ダンジョン配置
        Container.Bind<IDungeonDeployer>()
            .To<DungeonDeployer>()
            .FromNew()
            .AsSingle()
            .NonLazy();

        // コンテンツ配置
        Container.Bind<IDungeonContentsDeployer>()
            .To<DungeonContentsDeployer>()
            .FromNew()
            .AsSingle()
            .NonLazy();

        // ダンジョンアクセス用
        Container.Bind<IDungeonHandler>()
            .To<DungeonHandler>()
            .FromNew()
            .AsSingle()
            .NonLazy();

        // 初期化もするか
        if (m_Initialize == true)
        {
            // ダンジョン進行度管理
            Container.Bind(typeof(IDungeonProgressManager), typeof(IInitializable)) // 引数にIInitializableの型を渡す
                .To<DungeonProgressManager>()
                .FromNew()
                .AsSingle()
                .NonLazy();
        }
        else
        {
            // ダンジョン進行度管理
            Container.Bind<IDungeonProgressManager>()
                .To<DungeonProgressManager>()
                .FromNew()
                .AsSingle()
                .NonLazy();
        }

        // オブジェクトプール
        Container.Bind<IObjectPoolController>()
            .To<ObjectPoolController>()
            .FromNew()
            .AsSingle()
            .NonLazy();


        // アイテム管理
        Container.Bind<IItemManager>()
            .To<ItemManager>()
            .FromNew()
            .AsSingle()
            .NonLazy();

        // ユニットターン
        Container.Bind<ITurnManager>()
            .To<TurnManager>()
            .FromNew()
            .AsSingle()
            .NonLazy();

        // ユニット保持
        Container.Bind<IUnitHolder>()
            .To<UnitHolder>()
            .FromNew()
            .AsSingle()
            .NonLazy();

        // ユニット取得
        Container.Bind<IUnitFinder>()
            .To<UnitFinder>()
            .FromNew()
            .AsSingle()
            .NonLazy();

        // サウンド保持
        Container.Bind<ISoundHolder>()
            .To<SoundHolder>()
            .FromNew()
            .AsSingle()
            .NonLazy();


        // メニュー
        Container.Bind<IMenuUiManager>()
            .To<MenuUiManager>()
            .FromComponentOn(m_DungeonUiSystem)
            .AsSingle()
            .NonLazy();

        // バッグ
        Container.Bind<IBagUiManager>()
            .To<BagUiManager>()
            .FromComponentOn(m_DungeonUiSystem)
            .AsSingle()
            .NonLazy();

        // はい・いいえ
        Container.Bind<IYesorNoQuestionUiManager>()
            .To<YesorNoQuestionUiManager>()
            .FromComponentOn(m_DungeonUiSystem)
            .AsSingle()
            .NonLazy();

        // キャラUi
        Container.Bind<ICharaUiManager>()
            .To<CharaUiManager>()
            .FromComponentOn(m_DungeonUiSystem)
            .AsSingle()
            .NonLazy();

        // バトルログ
        Container.Bind<IBattleLogManager>()
            .To<BattleLogManager>()
            .FromComponentOn(m_DungeonUiSystem)
            .AsSingle()
            .NonLazy();

        // ダメージテキスト
        Container.Bind<IAttackResultUiManager>()
            .To<AttackResultUiManager>()
            .FromComponentOn(m_DungeonUiSystem)
            .AsSingle()
            .NonLazy();


        // インベントリ
        Container.Bind<ITeamInventory>()
            .To<TeamInventory>()
            .FromNew()
            .AsSingle()
            .NonLazy();

        // チームレベル
        Container.Bind(typeof(ITeamLevelHandler), typeof(IInitializable))
            .To<TeamLevelHandler>()
            .FromNew()
            .AsSingle()
            .NonLazy();
    }
}
