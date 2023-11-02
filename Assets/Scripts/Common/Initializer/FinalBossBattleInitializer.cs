using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System.Threading.Tasks;
using Zenject.SpaceFighter;
using Zenject;

public class FinalBossBattleInitializer : SceneInitializer
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
    private ITimelineManager m_TimelineManager;

    private FinalBossBattleSetup m_FinalBossBattleSetup;
    private GameObject m_Stage;

    private Vector3 BossPos { get; set; }

    private static readonly float OFFSET_Y = 0.5f;
    private static readonly Vector3[] WARRIOR_POS = new Vector3[7]
    {
        new Vector3(9f, OFFSET_Y, 13f), new Vector3(7f, OFFSET_Y, 11f), new Vector3(9f, OFFSET_Y, 9f), new Vector3(11f, OFFSET_Y, 7f),
        new Vector3(13f, OFFSET_Y, 9f), new Vector3(15f, OFFSET_Y, 11f), new Vector3(13f, OFFSET_Y, 13f),
    };
    private static readonly DIRECTION[] WARRIOR_DIR = new DIRECTION[7]
    {
        DIRECTION.LOWER_RIGHT, DIRECTION.RIGHT, DIRECTION.UPPER_RIGHT, DIRECTION.UP,
        DIRECTION.UPPER_LEFT, DIRECTION.LEFT, DIRECTION.LOWER_LEFT
    };
    private static readonly Vector3[] FRIEND_POS = new Vector3[3]
    {
        new Vector3(10f, OFFSET_Y, 11f), new Vector3(12f, OFFSET_Y, 11f), new Vector3(11f, OFFSET_Y, 10f)
    };
    private static readonly DIRECTION[] FRIEND_DIR = new DIRECTION[3]
    {
        DIRECTION.LEFT, DIRECTION.RIGHT, DIRECTION.UNDER
    };

    private void Awake()
    {
        MessageBroker.Default.Receive<FinishTimelineReadyToBossBattleMessage>().SubscribeWithState(this, (_, self) => self.OnReadyToBossBattleMessage()).AddTo(this);
    }

    /// <summary>
    /// スタート処理
    /// </summary>
    protected override Task OnStart()
    {
        m_FinalBossBattleSetup = m_DungeonProgressHolder.FinalBossBattleSetup;
        m_Stage = Instantiate(m_FinalBossBattleSetup.Stage);

        // Ui非表示
        MessageBroker.Default.Publish(new BattleUiSwitch(false));

        int progress = m_DungeonProgressHolder.CurrentDungeonProgress;

        // 負けフラグ回収
        if (m_FinalBossBattleSetup.IsLoseBack == true)
        {
            m_FinalBossBattleSetup.IsLoseBack = false;
            m_FinalBossBattleSetup.IsLoseBackComplete = true;
            m_TimelineManager.Play(TIMELINE_TYPE.KING_HELPER);
        }
        else if (m_InGameProgressHolder.DefeatBoss == false)
        {
            // イントロタイムライン再生
            var type = progress switch
            {
                1 => TIMELINE_TYPE.KING_INTRO,
                2 => TIMELINE_TYPE.BARM_INTRO,
                _ => TIMELINE_TYPE.NONE,
            };
            m_TimelineManager.Play(type);
        }
        else
        {
            // 撃破後フロー再生
            m_InGameProgressHolder.DefeatBoss = false;

            // イントロタイムライン再生
            var type = progress switch
            {
                1 => TIMELINE_TYPE.KING_DEFEAT,
                2 => TIMELINE_TYPE.BARM_DEFEAT,
                _ => TIMELINE_TYPE.NONE,
            };
            m_TimelineManager.Play(type);
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// ボスバトル開始
    /// </summary>
    private void OnReadyToBossBattleMessage()
    {
        m_FadeManager.StartFadeWhite(this, async self =>
        {
            await self.ReadyToBossBattle();
            self.m_TimelineManager.Finish();
        },
        this, self => self.m_TurnManager.NextUnitAct());
    }

    /// <summary>
    /// インゲーム準備
    /// </summary>
    private async Task ReadyToBossBattle()
    {
        // 前データ適用
        m_DungeonCharacterProgressManager.AdoptSaveData();

        int progress = m_DungeonProgressHolder.CurrentDungeonProgress;
        // キング戦
        if (progress == 1)
            await ReadyToKingBossBattle(m_FinalBossBattleSetup.IsLoseBackComplete);
        // バルム戦
        else if (progress == 2)
            await ReadyToKingBossBattle(m_FinalBossBattleSetup.IsLoseBack);
    }

    /// <summary>
    /// キング戦
    /// </summary>
    /// <param name="loseBack"></param>
    /// <returns></returns>
    private async Task ReadyToKingBossBattle(bool loseBack)
    {
        BossPos = new Vector3(11f, OFFSET_Y, 15f);

        Destroy(m_Stage);

        var bossSetup = m_FinalBossBattleSetup.KingBossBattleSetup;
        await BossBattleInitializer.DeployBossMap(bossSetup, m_DungeonDeployer); // ステージ生成

        // BGM
        var bgm = Instantiate(bossSetup.BGM);
        m_BGMHandler.SetBGM(bgm);

        // Ui表示
        MessageBroker.Default.Publish(new BattleUiSwitch(true));

        // ボス
        var boss = bossSetup.BossCharacterSetup;

        // 敵がいなくなったら終了
        m_UnitHolder.OnEnemyRemove.SubscribeWithState(this, (count, self) =>
        {
            if (count != 0)
                return;
            self.m_DungeonProgressManager.FinishDungeon(FINISH_REASON.BOSS_DEAD);
        }).AddTo(this);

        // 初回挑戦時
        if (loseBack == false)
        {
            LeaderPos = new Vector3(11f, OFFSET_Y, 11f);

            var bossBattleDeployInfo = new KingBattleDeployInfo(LeaderPos, BossPos, boss,
                WARRIOR_POS, WARRIOR_DIR, m_FinalBossBattleSetup.WarriorSetup, null, null, null);

            await m_DungeonContentsDeployer.DeployBossBattleContents(bossBattleDeployInfo);
        }
        // リベンジ
        else
        {
            LeaderPos = new Vector3(11f, OFFSET_Y, 12f);
            var bossBattleDeployInfo = new KingBattleDeployInfo(LeaderPos, BossPos, boss,
                WARRIOR_POS, WARRIOR_DIR, m_FinalBossBattleSetup.WarriorSetup,
                 FRIEND_POS, FRIEND_DIR, m_FinalBossBattleSetup.FriendSetup);

            await m_DungeonContentsDeployer.DeployBossBattleContents(bossBattleDeployInfo);
        }
    }
}

public readonly struct KingBattleDeployInfo
{
    public KingBattleDeployInfo(Vector3 playerPos, Vector3 bossPos, CharacterSetup bossCharacterSetup, Vector3[] warriorPos, DIRECTION[] warriorDir, CharacterSetup warriorSetup,
        Vector3[] friendPos, DIRECTION[] friendDir, CharacterSetup[] friendSetup)
    {
        PlayerPos = playerPos;
        BossPos = bossPos;
        BossCharacterSetup = bossCharacterSetup;
        WarriorPos = warriorPos;
        WarriorDir = warriorDir;
        WarriorSetup = warriorSetup;
        FriendPos = friendPos;
        FriendDir = friendDir;
        FriendSetup = friendSetup;
    }

    /// <summary>
    /// プレイヤーの初期位置
    /// </summary>
    public Vector3 PlayerPos { get; }

    /// <summary>
    /// ボスの位置
    /// </summary>
    public Vector3 BossPos { get; }

    /// <summary>
    /// ボスキャラセットアップ
    /// </summary>
    public CharacterSetup BossCharacterSetup { get; }

    /// <summary>
    /// ウォーリアの位置
    /// </summary>
    public Vector3[] WarriorPos { get; }
    public DIRECTION[] WarriorDir { get; }

    /// <summary>
    /// ウォーリアセットアップ
    /// </summary>
    public CharacterSetup WarriorSetup { get; }

    /// <summary>
    /// 助っ人の位置
    /// </summary>
    public Vector3[] FriendPos { get; }
    public DIRECTION[] FriendDir { get; }

    /// <summary>
    /// ボスキャラセットアップ
    /// </summary>
    public CharacterSetup[] FriendSetup { get; }
}

public readonly struct FinishTimelineReadyToBossBattleMessage
{

}