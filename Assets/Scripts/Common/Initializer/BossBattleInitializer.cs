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
    private IUnitHolder m_UnitHolder;

    protected override string FungusMessage => "BossBattleStart";

    protected override Vector3 LeaderStartPos => new Vector3(10f, OFFSET_Y, 5f);
    protected override Vector3 LeaderEndPos => new Vector3(10f, OFFSET_Y, 10f);

    protected override Vector3 FriendStartPos => new Vector3(12f, OFFSET_Y, 5f);
    protected override Vector3 FriendEndPos => new Vector3(12f, OFFSET_Y, 10f);

    private static readonly Vector3 BOSS_POS = new Vector3(11f, OFFSET_Y, 13f);

    /// <summary>
    /// ボスインスタンス
    /// </summary>
    private ICollector m_Boss;

    /// <summary>
    /// Fungusフロー
    /// </summary>
    private Fungus.Flowchart m_DefeatedFlowChart;

    /// <summary>
    /// スタート処理
    /// </summary>
    protected override void OnStart()
    {
        var bossBattleSetup = m_DungeonProgressHolder.CurrentBossBattleSetup;

        // 明転
        m_FadeManager.TurnBright(() => _ = OnTurnBright(), bossBattleSetup.BossBattleName, bossBattleSetup.WhereName);

        // 会話フロー生成
        m_ArrivalFlowChart = Instantiate(bossBattleSetup.ArrivalFlow).GetComponent<Fungus.Flowchart>();
        m_DefeatedFlowChart = Instantiate(bossBattleSetup.DefeatedFlow).GetComponent<Fungus.Flowchart>();

        base.OnStart();

        // ボス
        var boss = bossBattleSetup.BossCharacterSetup;
        var b = Instantiate(boss.OutGamePrefab);
        b.transform.position = BOSS_POS;
        m_Boss = b.GetComponent<ActorComponentCollector>();
        var controller = m_Boss.GetInterface<ICharaController>();
        controller.Face(DIRECTION.UNDER);

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

        var elementSetup = bossBattleSetup.ElementSetup;

        m_DungeonDeployer.DeployDungeon(cellMap, range, elementSetup);
    }

    /// <summary>
    /// 会話開始前
    /// </summary>
    async protected override Task OnTurnBright()
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
    public override void ReadyToOperatable()
    {
        m_FadeManager.StartFadeWhite(() => ReadyToBossBattle());
    }

    /// <summary>
    /// 
    /// </summary>
    private void ReadyToBossBattle()
    {
        m_Leader.Dispose();
        m_Friend.Dispose();
        m_Boss.Dispose();

        // ボス
        var bossBattleSetup = m_DungeonProgressHolder.CurrentBossBattleSetup;
        var boss = bossBattleSetup.BossCharacterSetup;
        var bossBattleDeployInfo = new BossBattleDeployInfo(LeaderEndPos, FriendEndPos, BOSS_POS, boss);

        m_DungeonContentsDeployer.DeployBossBattleContents(bossBattleDeployInfo);

        // BGM
        var bgm = Instantiate(bossBattleSetup.BGM);
        m_BGMHandler.SetBGM(bgm);

        // 敵がいなくなったら終了
        m_UnitHolder.OnEnemyRemove.Subscribe(count =>
        {
            if (count != 0)
                return;
            m_DungeonProgressManager.FinishDungeon(FINISH_REASON.BOSS_DEAD);
        }).AddTo(this);
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