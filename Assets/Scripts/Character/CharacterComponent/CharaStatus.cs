using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using NaughtyAttributes;
using Zenject;
using UniRx;
using Zenject.SpaceFighter;

public interface ICharaStatus : IActorInterface
{
    /// <summary>
    /// 敵レベルに上乗せするレベル
    /// </summary>
    int EnemyLevelBase { set; }

    /// <summary>
    /// ステータスセット
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    void SetStatus(CurrentStatus status);

    /// <summary>
    /// ステータスセット
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    void SetStatus(CharacterSetup setup);

    /// <summary>
    /// 現在のステータス
    /// </summary>
    CurrentStatus CurrentStatus { get; }

    /// <summary>
    /// 死んでいるか
    /// </summary>
    bool IsDead { get; }
}

public class CharaStatus : ActorComponentBase, ICharaStatus
{
    [Inject]
    private ITeamLevelHandler m_TeamLevelHandler;
    [Inject]
    private IDungeonProgressManager m_DungeonProgressManager;
    [Inject]
    private IUnitHolder m_UnitHolder;
    [Inject]
    private IMiniMapRenderer m_MiniMapRenderer;

    private int EnemyLevelBase { get; set; }
    int ICharaStatus.EnemyLevelBase { set => EnemyLevelBase = value; }

    private static readonly int ENEMY_RATIO = 3;

    /// <summary>
    /// 現在のステータス
    /// </summary>
    [SerializeField, ReadOnly]
    private CurrentStatus m_CurrentStatus;
    CurrentStatus ICharaStatus.CurrentStatus => m_CurrentStatus;

    bool ICharaStatus.IsDead => m_CurrentStatus.Hp == 0;

    protected override void Register(ICollector owner)
    {
        base.Register(owner);
        owner.Register(this);
    }

    /// <summary>
    /// ステータスセット
    /// </summary>
    /// <param name="status"></param>
    void ICharaStatus.SetStatus(CurrentStatus status)
    {
        m_CurrentStatus = status;
        if (m_CurrentStatus.OriginParam is PlayerStatus.PlayerParameter)
            PostSetFriendStatus();
        else if (m_CurrentStatus.OriginParam is EnemyStatus.EnemyParameter)
            PostSetEnemyStatus(m_CurrentStatus.OriginParam as EnemyStatus.EnemyParameter);
    }

    /// <summary>
    /// ステータスセット
    /// </summary>
    /// <param name="setup"></param>
    /// <returns></returns>
    void ICharaStatus.SetStatus(CharacterSetup setup)
    {
        var status = setup.Status;

        if (status is PlayerStatus)
            SetFriendStatus(setup, status as PlayerStatus);
        else if (status is EnemyStatus)
            SetEnemyStatus(setup, status as EnemyStatus);
    }

    /// <summary>
    /// 味方ステータスセット
    /// </summary>
    /// <param name="playerStatus"></param>
    private void SetFriendStatus(CharacterSetup setup, PlayerStatus playerStatus)
    {
        BattleStatus.Parameter param = new BattleStatus.Parameter(playerStatus.Param);
        int level = m_TeamLevelHandler.Level;
        m_CurrentStatus = new CurrentStatus(setup, param, level);

        PostSetFriendStatus();
    }
    private void PostSetFriendStatus()
    {
        // タイプセット
        if (Owner.RequireInterface<ICharaTypeHolder>(out var type) == true)
        {
            type.Type = CHARA_TYPE.FRIEND;
            type.TargetType = CHARA_TYPE.ENEMY;
        }

        // 死亡時にダンジョンシーンを抜ける
        if (Owner.RequireEvent<ICharaBattleEvent>(out var battle) == true)
        {
            battle.OnDead.SubscribeWithState(this, (_, self) => self.m_DungeonProgressManager.FinishDungeon(FINISH_REASON.PLAYER_DEAD)).AddTo(Owner.Disposables);
        }

        // レベル変動時にステータスに反映
        m_TeamLevelHandler.OnLevelChanged.SubscribeWithState(this, (_, self) =>
        {
            self.m_CurrentStatus.Lv = self.m_TeamLevelHandler.Level;
        }).AddTo(Owner.Disposables);
    }

    /// <summary>
    /// 敵ステータスセット
    /// </summary>
    /// <param name="enemyStatus"></param>
    private void SetEnemyStatus(CharacterSetup setup, EnemyStatus enemyStatus)
    {
        BattleStatus.Parameter param = new BattleStatus.Parameter(enemyStatus.Param);
        int level = (m_DungeonProgressManager.CurrentFloor + EnemyLevelBase) * ENEMY_RATIO;
        m_CurrentStatus = new CurrentStatus(setup, param, level);

        PostSetEnemyStatus(enemyStatus.Param);
    }
    private void PostSetEnemyStatus(EnemyStatus.EnemyParameter enemyParam)
    {
        // タイプセット
        if (Owner.RequireInterface<ICharaTypeHolder>(out var type) == true)
        {
            type.Type = CHARA_TYPE.ENEMY;
            type.TargetType = CHARA_TYPE.FRIEND;
        }

        // 死亡時に経験値を与える
        if (Owner.RequireEvent<ICharaBattleEvent>(out var battle) == true)
        {
            battle.OnDead.SubscribeWithState2(this, enemyParam, (result, self, enemyStatus) =>
            {
                foreach (var unit in self.m_UnitHolder.FriendList)
                {
                    // 味方キャラによるキルなら経験値加算
                    if (result.Attacker == unit)
                    {
                        self.m_TeamLevelHandler.AddExperience(enemyParam.Ex);
                        break;
                    }
                }
            }).AddTo(Owner.Disposables);
        }
    }
}