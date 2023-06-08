using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System.Threading.Tasks;
using Zenject;

/// <summary>
/// キャラタイプ
/// </summary>
public enum CHARA_TYPE
{
    NONE,
    PLAYER,
    ENEMY,
}

public interface ICharaTypeHolder : IActorInterface
{
    /// <summary>
    /// 自分のタイプ
    /// </summary>
    CHARA_TYPE Type { get; set; }

    /// <summary>
    /// 敵対タイプ
    /// </summary>
    CHARA_TYPE TargetType { get; set; }
}

public class CharaTypeHolder : ActorComponentBase, ICharaTypeHolder
{
    [Inject]
    private IDungeonProgressManager m_DungeonProgressManager;

    /// <summary>
    /// 味方か敵か
    /// </summary>
    [SerializeField, NaughtyAttributes.ReadOnly]
    private CHARA_TYPE m_Type = CHARA_TYPE.NONE;
    CHARA_TYPE ICharaTypeHolder.Type { get => m_Type; set => m_Type = value; }

    /// <summary>
    /// ターゲット
    /// </summary>
    [SerializeField, NaughtyAttributes.ReadOnly]
    private CHARA_TYPE m_TargetType = CHARA_TYPE.NONE;
    CHARA_TYPE ICharaTypeHolder.TargetType { get => m_TargetType; set => m_TargetType = value; }

    protected override void Register(ICollector owner)
    {
        base.Register(owner);
        owner.Register<ICharaTypeHolder>(this);
    }

    protected override void Initialize()
    {
        base.Initialize();

        // プレイヤー
        if (Owner.RequireInterface<IPlayerInput>(out var _) == true)
        {
            m_Type = CHARA_TYPE.PLAYER;
            m_TargetType = CHARA_TYPE.ENEMY;
        }
        // バディ
        else if (Owner.RequireInterface<IFriendAi>(out var _) == true)
        {
            m_Type = CHARA_TYPE.PLAYER;
            m_TargetType = CHARA_TYPE.ENEMY;
        }
        // 敵
        else if (Owner.RequireInterface<IEnemyAi>(out var _) == true)
        {
            m_Type = CHARA_TYPE.ENEMY;
            m_TargetType = CHARA_TYPE.PLAYER;
        }

        // プレイヤーなら死亡時にゲーム終了の処理
        if (m_Type == CHARA_TYPE.PLAYER)
        {
            var battle = Owner.GetEvent<ICharaBattleEvent>();
            battle.OnDead.SubscribeWithState(this, (_, self) => self.m_DungeonProgressManager.FinishDungeon(FINISH_REASON.PLAYER_DEAD)).AddTo(CompositeDisposable);
        }
    }
}