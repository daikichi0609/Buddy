using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System.Threading.Tasks;
using NaughtyAttributes;

/// <summary>
/// キャラの行動
/// </summary>
public enum CHARA_ACTION
{
    NONE,
    ATTACK,
    MOVE,
    WAIT,
    ITEM_USE,
    SKILL,
}

public interface ICharaLastActionHolder : IActorInterface
{
    /// <summary>
    /// 最新の行動
    /// </summary>
    CHARA_ACTION LastAction { get; }

    /// <summary>
    /// アクション登録
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    bool RegisterAction(CHARA_ACTION action);

    /// <summary>
    /// アクションリセット
    /// </summary>
    void Reset();
}

public class CharaLastActionHolder : ActorComponentBase, ICharaLastActionHolder
{
    /// <summary>
    /// 味方か敵か
    /// </summary>
    [SerializeField, ReadOnly]
    private CHARA_ACTION m_LastAction = CHARA_ACTION.NONE;
    CHARA_ACTION ICharaLastActionHolder.LastAction => m_LastAction;

    protected override void Register(ICollector owner)
    {
        base.Register(owner);
        owner.Register<ICharaLastActionHolder>(this);
    }

    bool ICharaLastActionHolder.RegisterAction(CHARA_ACTION action)
    {
        if (m_LastAction != CHARA_ACTION.NONE)
        {
            Debug.LogAssertion("すでに" + m_LastAction + "が登録済みです。" + action);
            return false;
        }

        m_LastAction = action;
        return true;
    }

    void ICharaLastActionHolder.Reset() => m_LastAction = CHARA_ACTION.NONE;
}