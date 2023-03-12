using UnityEngine;
using UniRx;
using System;
using System.Collections.Generic;

public interface ICharaAutoRecovery : IActorInterface
{

}

/// <summary>
/// 自動回復
/// </summary>
public class CharaAutoRecovery : ActorComponentBase, ICharaAutoRecovery
{
    private static readonly int RECOVER_TURN = 5;

    protected override void Register(ICollector owner)
    {
        base.Register(owner);
        owner.Register<ICharaAutoRecovery>(this);
    }

    protected override void Initialize()
    {
        base.Initialize();

        if (Owner.RequireEvent<ICharaTurnEvent>(out var turn) == true)
        {
            turn.OnTurnEndPost.Subscribe(_ =>
            {
                if (Owner.RequireInterface<ICharaStarvation>(out var starvation) == false || starvation.IsStarvate == true)
                    return;

                int currentTurn = TurnManager.Interface.TotalTurnCount + 1;

                // 回復インターバル
                if (currentTurn % RECOVER_TURN == 0)
                    return;

                if (Owner.RequireInterface<ICharaStatus>(out var status) == false)
                    return;

                status.CurrentStatus.Hp++;

            }).AddTo(Disposable);
        }
    }
}