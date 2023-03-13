using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using NaughtyAttributes;

public interface ICharaStatus : IActorInterface
{
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
    [SerializeField]
    private CHARA_NAME m_GivenName = CHARA_NAME.BOXMAN;

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

    protected override void Initialize()
    {
        base.Initialize();

        m_Parameter = CharaDataManager.LoadCharaParameter(m_GivenName);
        m_CurrentStatus = new CurrentStatus(m_Parameter);
    }
}