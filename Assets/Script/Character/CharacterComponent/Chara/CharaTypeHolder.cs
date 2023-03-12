using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System.Threading.Tasks;

public interface ICharaTypeHolder : IActorInterface
{
    CHARA_TYPE Type { get; set; }
}

public class CharaTypeHolder : ActorComponentBase, ICharaTypeHolder
{
    /// <summary>
    /// 味方か敵か
    /// </summary>
    private CHARA_TYPE m_Type = CHARA_TYPE.NONE;
    CHARA_TYPE ICharaTypeHolder.Type { get => m_Type; set => m_Type = value; }

    protected override void Register(ICollector owner)
    {
        base.Register(owner);
        owner.Register<ICharaTypeHolder>(this);
    }

    protected override void Initialize()
    {
        base.Initialize();

        if (Owner.RequireInterface<IAiAction>(out var enemy) == true)
            m_Type = CHARA_TYPE.ENEMY;
        else
            m_Type = CHARA_TYPE.PLAYER;
    }
}