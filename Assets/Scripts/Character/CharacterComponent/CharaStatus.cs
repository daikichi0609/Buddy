using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using NaughtyAttributes;
using Zenject;
using UniRx;
using System;
using System.Threading.Tasks;

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
    /// 回復（エフェクトあり）
    /// </summary>
    /// <param name="recover"></param>
    /// <returns></returns>
    Task RecoverHp(int recover);

    /// <summary>
    /// HPバーの色変更
    /// </summary>
    /// <param name="color"></param>
    /// <returns></returns>
    IDisposable ChangeBarColor(Color32 color);
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
    private ICharaUiManager m_CharaUiManager;
    [Inject]
    private IEffectHolder m_EffectHolder;
    [Inject]
    private ISoundHolder m_SoundHolder;
    [Inject]
    private IBattleLogManager m_BattleLogManager;

    private int EnemyLevelBase { get; set; }
    int ICharaStatus.EnemyLevelBase { set => EnemyLevelBase = value; }

    private static readonly int ENEMY_RATIO = 3;
    private static readonly string HEAL = "Heal";

    /// <summary>
    /// 現在のステータス
    /// </summary>
    [SerializeField, ReadOnly]
    private CurrentStatus m_CurrentStatus;
    CurrentStatus ICharaStatus.CurrentStatus => m_CurrentStatus;

    protected override void Register(ICollector owner)
    {
        base.Register(owner);
        owner.Register(this);
    }

    /// <summary>
    /// 回復
    /// </summary>
    /// <param name="recover"></param>
    /// <returns></returns>
    async Task ICharaStatus.RecoverHp(int recover)
    {
        if (m_EffectHolder.TryGetEffect(HEAL, out var effect) == true)
            effect.Play(Owner);
        if (m_SoundHolder.TryGetSound(HEAL, out var sound) == true)
            sound.Play();

        await Task.Delay(500);

        int d = m_CurrentStatus.RecoverHp(recover);
        m_BattleLogManager.Log(m_CurrentStatus.OriginParam.GivenName + "の体力は" + d.ToString() + "回復した！");
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
        bool isPlayer = status is PlayerStatus;

        if (isPlayer == true)
            SetFriendStatus(setup, status as PlayerStatus);
        else
            SetEnemyStatus(setup, status as EnemyStatus);

        if (setup.SkillSetup != null)
        {
            var skillHandler = Owner.GetInterface<ICharaSkillHandler>();
            foreach (var skill in setup.SkillSetup.SkillEffects)
            {
                var disposable = skillHandler.RegisterSkill(skill);
                if (isPlayer == false)
                    Owner.Disposables.Add(disposable);
            }

        }

        if (setup.ClevernesssSetup != null)
        {
            var clevernessHandler = Owner.GetInterface<ICharaClevernessHandler>();
            foreach (var cleverness in setup.ClevernesssSetup.ClevernessEffects)
            {
                var disposable = clevernessHandler.RegisterCleverness(cleverness);
                if (isPlayer == false)
                    Owner.Disposables.Add(disposable);
            }
        }
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
            battle.OnDead.SubscribeWithState(this, async (_, self) => await self.m_DungeonProgressManager.FinishDungeon(FINISH_REASON.PLAYER_DEAD)).AddTo(Owner.Disposables);
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

    IDisposable ICharaStatus.ChangeBarColor(Color32 color) => m_CharaUiManager.TryGetCharaUi(this, out var ui) ? ui.ChangeBarColor(color) : Disposable.Empty;
}