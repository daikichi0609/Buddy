using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using NaughtyAttributes;

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
    bool SetStatus(CharacterSetup setup);

    /// <summary>
    /// 現在のステータス
    /// </summary>
    CurrentStatus CurrentStatus { get; }

    /// <summary>
    /// 元パラメタ
    /// </summary>
    BattleStatus.Parameter Parameter { get; }

    /// <summary>
    /// 死んでいるか
    /// </summary>
    bool IsDead { get; }
}

public class CharaStatus : ActorComponentBase, ICharaStatus
{
    /// <summary>
    /// セットアップ
    /// </summary>
    private CharacterSetup m_Setup;
    CharacterSetup ICharaStatus.Setup => m_Setup;

    /// <summary>
    /// 元パラメータ
    /// </summary>
    private BattleStatus.Parameter m_Parameter;
    BattleStatus.Parameter ICharaStatus.Parameter => m_Parameter;

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
    /// プレイヤーの場合、すでにセット済みならセットしない
    /// </summary>
    /// <param name="setup"></param>
    /// <returns></returns>
    bool ICharaStatus.SetStatus(CharacterSetup setup)
    {
        m_Setup = setup;
        var status = m_Setup.Status;

        if (status is PlayerStatus)
        {
            var s = status as PlayerStatus;
            m_Parameter = new BattleStatus.Parameter(s.Param);
        }
        else if (status is EnemyStatus)
        {
            var s = status as EnemyStatus;
            m_Parameter = new BattleStatus.Parameter(s.Param);
        }

        m_CurrentStatus = new CurrentStatus(m_Parameter);
        return m_Parameter != null;
    }
}