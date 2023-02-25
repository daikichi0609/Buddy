using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public interface ICharaStatus : ICharacterComponent
{
    /// <summary>
    /// 現在のステータス
    /// </summary>
    CurrentStatus CurrentStatus { get; }

    /// <summary>
    /// 元パラメタ
    /// </summary>
    BattleStatus.Parameter Parameter { get; }
}

public class CharaStatus : CharaComponentBase, ICharaStatus
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
    private CurrentStatus m_CurrentStatus;
    CurrentStatus ICharaStatus.CurrentStatus => m_CurrentStatus;

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