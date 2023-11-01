using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System.Threading.Tasks;
using Zenject.SpaceFighter;
using Zenject;

public class BossBattleInitializer : SceneInitializer
{
    [Inject]
    private IDungeonDeployer m_DungeonDeployer;
    [Inject]
    private IDungeonContentsDeployer m_DungeonContentsDeployer;
    [Inject]
    protected IDungeonProgressManager m_DungeonProgressManager;
    [Inject]
    private IDungeonCharacterProgressManager m_DungeonCharacterProgressManager;
    [Inject]
    private IUnitHolder m_UnitHolder;
    [Inject]
    private ITurnManager m_TurnManager;
    [Inject]
    private DungeonCharacterProgressSaveData m_DungeonCharacterSaveData;

    protected override string FungusMessage => "BossBattleStart";
    private static readonly string ms_DefeatMessage = "Defeat";
    private static readonly float OFFSET_Y = 0.5f;

    private Vector3 LeaderStartPos { get; set; }
    private Vector3 FriendStartPos { get; set; }
    private Vector3 LeaderEndPos { get; set; }
    private Vector3 FriendEndPos { get; set; }

    private Vector3 BossPos { get; set; } = new Vector3(11f, OFFSET_Y, 13f);

    /// <summary>
    /// Fungusフロー
    /// </summary>
    private Fungus.Flowchart m_ArrivalFlowChart;
    private Fungus.Flowchart m_DefeatedFlowChart;

    /// <summary>
    /// ボスインスタンス
    /// </summary>
    private ICollector m_Boss;

    private void Awake()
    {
        LeaderStartPos = new Vector3(10f, OFFSET_Y, 5f);
        FriendStartPos = new Vector3(12f, OFFSET_Y, 5f);
        LeaderEndPos = new Vector3(10f, OFFSET_Y, 10f);
        FriendEndPos = new Vector3(12f, OFFSET_Y, 10f);

        /*
        MessageBroker.Default.Receive<BossBattleInitializerInfo>().Subscribe(info =>
        {

        }).AddTo(this);
        */
    }

    /// <summary>
    /// スタート処理
    /// </summary>
    protected override async Task OnStart()
    {
        var bossBattleSetup = m_DungeonProgressHolder.CurrentBossBattleSetup;
        await DeployBossMap(bossBattleSetup, m_DungeonDeployer); // ステージ生成

        BossSetup(bossBattleSetup, BossPos); // ボスキャラ生成

        // Ui非表示
        MessageBroker.Default.Publish(new BattleUiSwitch(false));

        // 明転
        if (m_InGameProgressHolder.DefeatBoss == false)
        {
            CreateOutGameCharacter(LeaderStartPos, FriendStartPos); // キャラ生成
            await m_FadeManager.TurnBright(this, async self => await self.OnTurnBright(), bossBattleSetup.BossBattleName, bossBattleSetup.WhereName);
        }
        else
        {
            m_InGameProgressHolder.DefeatBoss = false; // フラグ解除
            CreateOutGameCharacter(LeaderEndPos, FriendEndPos); // キャラ生成
            m_Leader.GetInterface<ICharaController>().Face(DIRECTION.UP);
            m_Friend.GetInterface<ICharaController>().Face(DIRECTION.UP);
            m_Boss.GetInterface<ICharaAnimator>().PlayAnimation(ANIMATION_TYPE.SLEEP);

            m_DungeonCharacterSaveData.ResetData(); // キャラデータリセット
            m_DungeonProgressHolder.CurrentDungeonProgress = 0; // ダンジョン進行度リセット
            m_InGameProgressHolder.InGameProgress++; // 進行度Up
            await m_FadeManager.TurnBright(this, self => self.DefeatBoss());
        }
    }

    /// <summary>
    /// 会話開始前
    /// </summary>
    private async Task OnTurnBright()
    {
        // コントローラー取得
        ICharaController leader = m_Leader.GetInterface<ICharaController>();
        ICharaController friend = m_Friend.GetInterface<ICharaController>();

        // 定点移動
        var leaderMove = leader.MoveToPoint(LeaderEndPos, 3f);
        var friendMove = friend.MoveToPoint(FriendEndPos, 3f);
        await Task.WhenAll(leaderMove, friendMove);
        await Task.Delay(500);

        // 会話開始
        m_ArrivalFlowChart.SendFungusMessage(FungusMessage);
    }

    /// <summary>
    /// 操作可能にする
    /// </summary>
    public override void FungusMethod()
    {
        m_FadeManager.StartFadeWhite(this, async self => await self.ReadyToBossBattle(), this, self => self.m_TurnManager.NextUnitAct());
    }

    /// <summary>
    /// ボスキャラ生成
    /// 会話フロー生成
    /// </summary>
    /// <param name="setup"></param>
    /// <param name="pos"></param>
    /// <returns></returns>
    private void BossSetup(BossBattleSetup setup, Vector3 pos)
    {
        // ボス
        var boss = setup.BossCharacterSetup;
        var b = m_Instantiater.InstantiatePrefab(boss.Prefab, ms_OutGameUnitInjector);
        b.transform.position = pos;
        m_Boss = b.GetComponent<ActorComponentCollector>();
        m_Boss.Initialize();
        var controller = m_Boss.GetInterface<ICharaController>();
        controller.Face(DIRECTION.UNDER);

        // 会話フロー生成
        m_ArrivalFlowChart = m_Instantiater.InstantiatePrefab(setup.ArrivalFlow).GetComponent<Fungus.Flowchart>();
        m_DefeatedFlowChart = m_Instantiater.InstantiatePrefab(setup.DefeatedFlow).GetComponent<Fungus.Flowchart>();
    }

    /// <summary>
    /// マップ生成
    /// </summary>
    /// <param name="setup"></param>
    /// <returns></returns>
    public static async Task DeployBossMap(BossBattleSetup setup, IDungeonDeployer dungeonDeployer)
    {
        // ダンジョン
        var cellMap = new TERRAIN_ID[21, 21];
        Range range = new Range(6, 6, 16, 16);

        for (int x = range.Start.X; x <= range.End.X; x++)
            for (int y = range.Start.Y; y <= range.End.Y; y++)
                cellMap[x, y] = TERRAIN_ID.ROOM;

        /*
        // https://blog.xin9le.net/entry/2013/12/14/033519
        // 0は暗黙変換できる！
        CELL_ID[,] map = new CELL_ID[21, 21] {
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        };
        */

        var elementSetup = setup.ElementSetup;

        await dungeonDeployer.DeployDungeon(cellMap, range, elementSetup);
    }

    /// <summary>
    /// インゲーム準備
    /// </summary>
    private async Task ReadyToBossBattle()
    {
        m_Leader.Dispose();
        m_Friend.Dispose();
        m_Boss.Dispose();

        // 前データ適用
        m_DungeonCharacterProgressManager.AdoptSaveData();

        // ボス
        var bossBattleSetup = m_DungeonProgressHolder.CurrentBossBattleSetup;
        var boss = bossBattleSetup.BossCharacterSetup;
        var bossBattleDeployInfo = new BossBattleDeployInfo(LeaderEndPos, FriendEndPos, BossPos, boss);

        await m_DungeonContentsDeployer.DeployBossBattleContents(bossBattleDeployInfo);

        // BGM
        var bgm = Instantiate(bossBattleSetup.BGM);
        m_BGMHandler.SetBGM(bgm);

        // Ui表示
        MessageBroker.Default.Publish(new BattleUiSwitch(true));

        // 敵がいなくなったら終了
        m_UnitHolder.OnEnemyRemove.SubscribeWithState(this, (count, self) =>
        {
            if (count != 0)
                return;

            self.m_DungeonProgressManager.FinishDungeon(FINISH_REASON.BOSS_DEAD);
        }).AddTo(this);
    }

    /// <summary>
    /// ボス撃破後
    /// </summary>
    private void DefeatBoss()
    {
        m_DungeonProgressHolder.CurrentDungeonProgress++;
        m_DefeatedFlowChart.SendFungusMessage(ms_DefeatMessage);
    }
}

public readonly struct BossBattleDeployInfo
{
    public BossBattleDeployInfo(Vector3 playerPos, Vector3 friendPos, Vector3 bossPos, CharacterSetup bossCharacterSetup)
    {
        PlayerPos = playerPos;
        FriendPos = friendPos;
        BossPos = bossPos;
        BossCharacterSetup = bossCharacterSetup;
    }

    /// <summary>
    /// プレイヤーの初期位置
    /// </summary>
    public Vector3 PlayerPos { get; }

    /// <summary>
    /// 敵の初期位置
    /// </summary>
    public Vector3 FriendPos { get; }

    /// <summary>
    /// ボスの位置
    /// </summary>
    public Vector3 BossPos { get; }

    /// <summary>
    /// ボスキャラセットアップ
    /// </summary>
    public CharacterSetup BossCharacterSetup { get; }
}