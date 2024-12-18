using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class DungeonCoreSystemInstaller : MonoInstaller
{
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

        // ダンジョン進行度管理
        Container.Bind<IDungeonProgressManager>()
            .To<DungeonProgressManager>()
            .FromNew()
            .AsSingle()
            .NonLazy();

        // ダンジョンキャラ状態保存
        Container.Bind<IDungeonCharacterProgressManager>()
            .To<DungeonCharacterProgressManager>()
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

        // ステータス保存
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

        // バトルログ確認
        Container.Bind<IBattleLogConfirmManager>()
            .To<BattleLogConfirmManager>()
            .FromComponentOn(m_DungeonUiSystem)
            .AsSingle()
            .NonLazy();

        // ダメージテキスト
        Container.Bind<IPopUpUiManager>()
            .To<PopUpUiManager>()
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
