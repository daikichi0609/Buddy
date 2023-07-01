using UnityEngine;
using UniRx;
using System;
using System.Collections.Generic;
using Zenject;

public interface ICharaAutoRecovery : IActorInterface
{
    void Reset();
}

/// <summary>
/// 自動回復
/// </summary>
public class CharaAutoRecovery : ActorComponentBase, ICharaAutoRecovery
{
    [Inject]
    private ITurnManager m_TurnManager;

    private static readonly int RECOVER_TURN = 10;

    private int m_TurnCount;

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
            turn.OnTurnEnd.SubscribeWithState(this, (_, self) =>
            {
                if (self.Owner.RequireInterface<ICharaStarvation>(out var starvation) == true && starvation.IsStarvate == true)
                    return;

                // 回復インターバル
                if (++m_TurnCount % RECOVER_TURN != 0)
                    return;

                if (self.Owner.RequireInterface<ICharaStatus>(out var status) == false)
                    return;

                status.CurrentStatus.RecoverHp(1);

            }).AddTo(Owner.Disposables);
        }
    }

    void ICharaAutoRecovery.Reset() => m_TurnCount = 0;
}