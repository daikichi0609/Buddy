using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public interface ICharaTurn : ICharacterComponent
{
    bool IsFinishTurn { get; }
    bool IsActing { get; }

    void StartTurn();
    void FinishTurn();
    void StartAction();
    void FinishAction();

}

public class CharaTurn : CharaComponentBase, ICharaTurn
{
    /// <summary>
    /// 行動済みステータス
    /// </summary>
    private bool IsFinishTurn { get; set; }
    bool ICharaTurn.IsFinishTurn => IsFinishTurn;

    /// <summary>
    /// 行動中ステータス
    /// 行動の終了 = ターン終了ではない場合に使う
    /// </summary>
    private bool IsActing { get; set; }
    bool ICharaTurn.IsActing => IsActing;

    /// <summary>
    /// ターン開始 行動可能状態になる
    /// </summary>
    void ICharaTurn.StartTurn() => IsFinishTurn = false;

    /// <summary>
    /// ターン終了 行動済み状態になる
    /// </summary>
    void ICharaTurn.FinishTurn()
    {
        IsFinishTurn = true;
        TurnManager.Interface.NextUnitAct();
    }

    /// <summary>
    /// アクション開始 アクション中状態になる
    /// </summary>
    void ICharaTurn.StartAction() => IsActing = true;

    /// <summary>
    /// アクション終わり
    /// </summary>
    void ICharaTurn.FinishAction() => IsActing = false;
}
