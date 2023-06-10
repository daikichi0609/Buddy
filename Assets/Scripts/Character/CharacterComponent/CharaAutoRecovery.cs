using UnityEngine;
using UniRx;
using System;
using System.Collections.Generic;
using Zenject;

public interface ICharaAutoRecovery : IActorInterface
{

}

/// <summary>
/// 自動回復
/// </summary>
public class CharaAutoRecovery : ActorComponentBase, ICharaAutoRecovery
{
    [Inject]
    private ITurnManager m_TurnManager;

    private static readonly int RECOVER_TURN = 10;

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
            turn.OnTurnEndPost.SubscribeWithState(this, (_, self) =>
            {
                if (self.Owner.RequireInterface<ICharaStarvation>(out var starvation) == true && starvation.IsStarvate == true)
                    return;

                int currentTurn = self.m_TurnManager.TotalTurnCount + 1;

                // 回復インターバル
                if (currentTurn % RECOVER_TURN != 0)
                    return;

                if (self.Owner.RequireInterface<ICharaStatus>(out var status) == false)
                    return;

                status.CurrentStatus.RecoverHp(1);

            }).AddTo(CompositeDisposable);
        }
    }
}