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
    private bool m_SpawnRandomEnemy;

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

        // ダンジョンキャラ状態保存
        Container.Bind<IDungeonCharacterProgressManager>()
            .To<DungeonCharacterProgressManager>()
            .FromNew()
            .AsSingle()
            .NonLazy();

        // オブジェクトプール
        Container.Bind<IObjectPoolController>()
            .To<ObjectPoolController>()
            .FromNew()
            .AsSingle()
            .NonLazy();

        // 味方生成
        Container.Bind<IDungeonFriendSpawner>()
            .To<DungeonFriendSpawner>()
            .FromNew()
            .AsSingle()
            .NonLazy();

        // 敵生成
        Container.Bind<IDungeonEnemySpawner>()
            .To<DungeonEnemySpawner>()
            .FromNew()
            .AsSingle()
            .NonLazy();

        // 敵自動沸き
        if (m_SpawnRandomEnemy == true)
        {
            Container.Bind(typeof(IDungeonEnemyAutoSpawner), typeof(IInitializable))
                .To<DungeonEnemyAutoSpawner>()
                .FromNew()
                .AsSingle()
                .NonLazy();
        }

        // アイテム生成
        Container.Bind<IDungeonItemSpawner>()
            .To<DungeonItemSpawner>()
            .FromNew()
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

        // インベントリ
        Container.Bind<ITeamStatusHolder>()
            .To<TeamStatusHolder>()
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
        Container.Bind(typeof(ITurnManager), typeof(IInitializable))
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

        // チームレベルUi
        Container.Bind<TeamLevelUiManager>()
            .To<TeamLevelUiManager>()
            .FromComponentOn(m_DungeonUiSystem)
            .AsSingle()
            .NonLazy();

        // ミニマップUi
        Container.Bind<IMiniMapRenderer>()
            .To<MiniMapRenderer>()
            .FromComponentOn(m_DungeonUiSystem)
            .AsSingle()
            .NonLazy();

        // アイテム使用Ui
        Container.Bind<IItemUseUiManager>()
            .To<ItemUseUiManager>()
            .FromComponentOn(m_DungeonUiSystem)
            .AsSingle()
            .NonLazy();

        // アイテム使用キャラ選択Ui
        Container.Bind<IUseCharaUiManager>()
            .To<UseCharaUiManager>()
            .FromComponentOn(m_DungeonUiSystem)
            .AsSingle()
            .NonLazy();

        // キャラスキル確認Ui
        Container.Bind<ICharaSkillUiManager>()
            .To<CharaSkillUiManager>()
            .FromComponentOn(m_DungeonUiSystem)
            .AsSingle()
            .NonLazy();

        // キャラかしこさ確認Ui
        Container.Bind<ICharaClevernessUiManager>()
            .To<CharaClevernessUiManager>()
            .FromComponentOn(m_DungeonUiSystem)
            .AsSingle()
            .NonLazy();

        // スキルCT確認Ui
        Container.Bind<SkillCoolTimeUiManager>()
            .To<SkillCoolTimeUiManager>()
            .FromComponentOn(m_DungeonUiSystem)
            .AsSingle()
            .NonLazy();
    }
}
