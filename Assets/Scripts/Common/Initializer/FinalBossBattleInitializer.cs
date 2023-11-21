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

    /// <summary>
    /// イントロタイムライン
    /// </summary>
    /// <param name="progress"></param>
    /// <returns></returns>
    private TIMELINE_TYPE GetCurrentIntroTimelineType(int progress)
    {
        return progress switch
        {
            1 => TIMELINE_TYPE.KING_INTRO,
            2 => TIMELINE_TYPE.BARM_INTRO,
            _ => TIMELINE_TYPE.NONE,
        };
    }

    /// <summary>
    /// 撃破タイムライン
    /// </summary>
    /// <param name="progress"></param>
    /// <returns></returns>
    private TIMELINE_TYPE GetCurrentDefeatTimelineType(int progress)
    {
        // イントロタイムライン再生
        return progress switch
        {
            1 => TIMELINE_TYPE.KING_DEFEAT,
            2 => TIMELINE_TYPE.BARM_DEFEAT,
            _ => TIMELINE_TYPE.NONE,
        };
    }

    /// <summary>
    /// ボスバトルセットアップ
    /// </summary>
    /// <param name="progress"></param>
    /// <returns></returns>
    private BossBattleSetup GetCurrentBossBattleSetup(int progress)
    {
        return progress switch
        {
            1 => m_FinalBossBattleSetup.KingBossBattleSetup,
            2 => m_FinalBossBattleSetup.BarmBossBattleSetup,
            _ => null
        };
    }

    private void Awake()
    {
        // ボスバトル開始
        MessageBroker.Default.Receive<ReadyToBossBattleMessage>().SubscribeWithState(this, (_, self) => self.OnReadyToBossBattleMessageReceive()).AddTo(this);

        // フレンド設定
        MessageBroker.Default.Receive<SetFriendMessage>().SubscribeWithState(this, (message, self) =>
        {
            int index = message.FriendName switch
            {
                CHARA_NAME.RAGON => 0,
                CHARA_NAME.BERRY => 1,
                CHARA_NAME.DORCHE => 2,
                _ => -1,
            };
            self.m_InGameProgressHolder.SetFinalBattleFriendIndex(index);
            self.m_InGameProgressHolder.NoFriend = false;
        }).AddTo(this);

        // ボスバトル開始
        MessageBroker.Default.Receive<GameClearMessage>().SubscribeWithState(this, (_, self) => self.OnGameClearMessageReceive()).AddTo(this);
    }

    /// <summary>
    /// スタート処理
    /// </summary>
    protected override async Task OnStart()
    {
        CreateOutGameCharacter(Vector3.zero, Vector3.zero);

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
        // イントロタイムライン再生
        else if (m_InGameProgressHolder.DefeatBoss == false)
        {
            var type = GetCurrentIntroTimelineType(progress);
            var bossBattleSetup = GetCurrentBossBattleSetup(progress);

            // 一旦タイムラインは無しで進める
            // バルム戦開始
            if (type == TIMELINE_TYPE.BARM_INTRO)
            {
                var _ = m_FadeManager.TurnBright(string.Empty, string.Empty);
                OnReadyToBossBattleMessageReceive();
                return;
            }

            await m_FadeManager.ShowWhere(bossBattleSetup.BossBattleName, bossBattleSetup.WhereName);
            m_TimelineManager.Play(type);
        }
        // 撃破後フロー再生
        else
        {
            m_InGameProgressHolder.DefeatBoss = false;
            var type = GetCurrentDefeatTimelineType(progress);

            // 一旦タイムラインは無しで進める
            // バルム戦後
            if (type == TIMELINE_TYPE.BARM_DEFEAT)
            {
                OnGameClearMessageReceive();
                return;
            }

            m_TimelineManager.Play(type);
        }
    }

    /// <summary>
    /// ボスバトル開始
    /// </summary>
    private void OnReadyToBossBattleMessageReceive()
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
        if (progress == 1) // キング戦
            await ReadyToKingBossBattle(m_FinalBossBattleSetup.IsLoseBackComplete);
        else if (progress == 2) // バルム戦
            await ReadyToBarmBossBattle();
#if DEBUG
        else
        {
            Debug.LogError("ダンジョン進行度が適切ではありません。");
        }
#endif
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

        // 敵がいなくなったら終了
        m_UnitHolder.OnEnemyRemove.SubscribeWithState(this, (count, self) =>
        {
            if (count != 0)
                return;
            self.m_DungeonProgressManager.FinishDungeon(FINISH_REASON.BOSS_DEAD);
        }).AddTo(this);

        // ボス
        var boss = bossSetup.BossCharacterSetup;

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

    /// <summary>
    /// バルム戦
    /// </summary>
    /// <param name="loseBack"></param>
    /// <returns></returns>
    private async Task ReadyToBarmBossBattle()
    {
        BossPos = new Vector3(11f, OFFSET_Y, 15f);

        Destroy(m_Stage);

        var bossSetup = m_FinalBossBattleSetup.BarmBossBattleSetup;
        await BossBattleInitializer.DeployBossMap(bossSetup, m_DungeonDeployer); // ステージ生成

        // BGM
        var bgm = Instantiate(bossSetup.BGM);
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

        LeaderPos = new Vector3(10f, OFFSET_Y, 11f);
        Vector3 friendPos = new Vector3(12f, OFFSET_Y, 11f);
        BossPos = new Vector3(11f, OFFSET_Y, 13f);
        var boss = bossSetup.BossCharacterSetup;
        var bossBattleDeployInfo = new BossBattleDeployInfo(LeaderPos, friendPos, BossPos, boss);

        await m_DungeonContentsDeployer.DeployBossBattleContents(bossBattleDeployInfo);
    }

    /// <summary>
    /// ゲームクリア時
    /// 全ての進行度をリセットして、はじめからにする
    /// </summary>
    private void OnGameClearMessageReceive()
    {
        m_InGameProgressHolder.ResetAll();
        m_DungeonProgressHolder.ResetAll();
        m_DungeonCharacterProgressManager.ResetAll();

        m_FadeManager.LoadScene(SceneName.SCENE_HOME);
    }
}

/// <summary>
/// ボスバトル開始
/// </summary>
public readonly struct ReadyToBossBattleMessage
{

}

/// <summary>
/// キング戦
/// </summary>
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

/// <summary>
/// ゲームクリア時
/// </summary>
public readonly struct GameClearMessage
{

}