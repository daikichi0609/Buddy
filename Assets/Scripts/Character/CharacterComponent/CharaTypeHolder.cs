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
    FRIEND,
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
}