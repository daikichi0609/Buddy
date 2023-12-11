using Zenject;

public class DungeonSystemInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        // 初期化
        Container.Bind<ISceneInitializer>()
            .To<DungeonInitializer>()
            .FromNewComponentOnNewGameObject()
            .AsSingle()
            .NonLazy();

        // 敵自動沸き
        Container.Bind(typeof(IDungeonEnemyAutoSpawner), typeof(IInitializable))
                .To<DungeonEnemyAutoSpawner>()
                .FromNew()
                .AsSingle()
                .NonLazy();
    }
}