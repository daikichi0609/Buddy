using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using NaughtyAttributes;
using Zenject;
using UniRx;

public interface ICharaStatus : IActorInterface
{
    /// <summary>
    /// セットアップ
    /// </summary>
    CharacterSetup Setup { get; }

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

    /// <summary>
    /// セットアップ
    /// </summary>
    private CharacterSetup m_Setup;
    CharacterSetup ICharaStatus.Setup => m_Setup;

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
    /// <param name="setup"></param>
    /// <returns></returns>
    void ICharaStatus.SetStatus(CharacterSetup setup)
    {
        m_Setup = setup;
        var status = m_Setup.Status;

        if (status is PlayerStatus)
            SetPlayerStatus(status as PlayerStatus);
        else if (status is EnemyStatus)
            SetEnemyStatus(status as EnemyStatus);
    }

    /// <summary>
    /// プレイヤー
    /// </summary>
    /// <param name="playerStatus"></param>
    private void SetPlayerStatus(PlayerStatus playerStatus)
    {
        BattleStatus.Parameter param = new BattleStatus.Parameter(playerStatus.Param);
        int level = m_TeamLevelHandler.Level;
        m_CurrentStatus = new CurrentStatus(param, level);

        // タイプセット
        if (Owner.RequireInterface<ICharaTypeHolder>(out var type) == true)
        {
            type.Type = CHARA_TYPE.FRIEND;
            type.TargetType = CHARA_TYPE.ENEMY;
        }

        // 死亡時にダンジョンシーンを抜ける
        if (Owner.RequireEvent<ICharaBattleEvent>(out var battle) == true)
        {
            battle.OnDead.SubscribeWithState(this, (_, self) => self.m_DungeonProgressManager.FinishDungeon(FINISH_REASON.PLAYER_DEAD)).AddTo(CompositeDisposable);
        }

        // レベル変動時にステータスに反映
        m_TeamLevelHandler.OnLevelChanged.SubscribeWithState(this, (_, self) =>
        {
            self.m_CurrentStatus.Lv = self.m_TeamLevelHandler.Level;
        }).AddTo(CompositeDisposable);
    }

    /// <summary>
    /// 敵ステータスセット
    /// </summary>
    /// <param name="enemyStatus"></param>
    private void SetEnemyStatus(EnemyStatus enemyStatus)
    {
        BattleStatus.Parameter param = new BattleStatus.Parameter(enemyStatus.Param);
        int level = m_DungeonProgressManager.CurrentFloor;
        m_CurrentStatus = new CurrentStatus(param, level);

        // タイプセット
        if (Owner.RequireInterface<ICharaTypeHolder>(out var type) == true)
        {
            type.Type = CHARA_TYPE.ENEMY;
            type.TargetType = CHARA_TYPE.FRIEND;
        }

        // 死亡時に経験値を与える
        if (Owner.RequireEvent<ICharaBattleEvent>(out var battle) == true)
        {
            battle.OnDead.SubscribeWithState2(this, enemyStatus, (result, self, enemyStatus) =>
            {
                foreach (var unit in self.m_UnitHolder.FriendList)
                {
                    // 味方キャラによるキルなら経験値加算
                    if (result.Attacker == unit)
                    {
                        self.m_TeamLevelHandler.AddExperience(enemyStatus.Param.Ex);
                        break;
                    }
                }
            }).AddTo(CompositeDisposable);
        }
    }
}