using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class CharaTurn : MonoBehaviour
{
    /// <summary>
    /// 行動済みステータス
    /// </summary>
    [SerializeField]
    private bool m_IsFinishTurn;
    public bool IsFinishTurn
    {
        get { return m_IsFinishTurn; }
        private set { m_IsFinishTurn = value; }
    }

    /// <summary>
    /// 行動中ステータス
    /// </summary>
    [SerializeField]
    private bool m_IsActing;
    public bool IsActing
    {
        get { return m_IsActing; }
        private set { m_IsActing = value; }
    }

    /// <summary>
    /// 全キャラに行動を禁止させるフラグ
    /// </summary>
    [SerializeField]
    private bool m_CAN_ATTACK = true;
    public bool CAN_ATTACK
    {
        get { return m_CAN_ATTACK; }
        set { m_CAN_ATTACK = value; }
    }

    /// <summary>
    /// 全キャラに攻撃を禁止させるフラグ
    /// </summary>
    [SerializeField]
    private bool m_CAN_ACTION = true;
    public bool CAN_ACTION
    {
        get { return m_CAN_ACTION; }
        set { m_CAN_ACTION = value; }
    }

    /// <summary>
    /// ターン終了 行動済み状態になる
    /// </summary>
    public void FinishTurn()
    {
        IsFinishTurn = true;
        MessageBroker.Default.Publish(new Message.MFinishTurn());
    }

    /// <summary>
    /// ターン開始 行動可能状態になる
    /// </summary>
    public void StartTurn()
    {
        IsFinishTurn = false;
    }

    /// <summary>
    /// アクション開始 アクション中状態になる
    /// </summary>
    public void StartAction()
    {
        IsActing = true;
    }

    /// <summary>
    /// アクション終わり
    /// </summary>
    public void FinishAction()
    {
        IsActing = false;
    }
}
