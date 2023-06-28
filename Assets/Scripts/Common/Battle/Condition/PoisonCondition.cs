using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class PoisonCondition : Condition
{
    private static readonly float POISON_DAMAGE_RATIO = 0.03f;
    public static readonly int POISON_REMAINING_TURN = 10;

    public PoisonCondition(int remainingTurn) : base(remainingTurn) { }

    protected override async Task OnStart(ICollector owner)
    {
        var status = owner.GetInterface<ICharaStatus>();
        string log = status.CurrentStatus.OriginParam.GivenName + "は毒状態になった！";
        owner.GetInterface<ICharaLog>().Log(log);
        await Task.Delay(500);
    }

    protected override async Task EffectInternal(ICollector owner)
    {
        AttackPercentageInfo info = new AttackPercentageInfo(default, default, POISON_DAMAGE_RATIO, 100f, DIRECTION.NONE);

        var turn = owner.GetInterface<ICharaTurn>();
        await turn.WaitFinishActing();
        await owner.GetInterface<ICharaBattle>().DamagePercentage(info);
    }

    protected override async Task OnFinish(ICollector owner)
    {
        m_OnFinish.Dispose();

        var status = owner.GetInterface<ICharaStatus>();
        string log = status.CurrentStatus.OriginParam.GivenName + "の毒は治った！";
        owner.GetInterface<ICharaLog>().Log(log);
        await Task.Delay(500);
    }
}