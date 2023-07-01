using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class PoisonCondition : Condition
{
    protected override bool CanOverlapping => false;

    private static readonly Color32 ms_BarColor = new Color32(167, 87, 168, 255);
    private static readonly float POISON_DAMAGE_RATIO = 0.03f;
    public static readonly int POISON_DAMAGE_INTERVAL = 3;
    public static readonly int POISON_REMAINING_TURN = 30;

    private int m_TurnCount;

    public PoisonCondition(int remainingTurn) : base(remainingTurn) { }

    protected override async Task OnStart(ICollector owner)
    {
        var status = owner.GetInterface<ICharaStatus>();
        string log = status.CurrentStatus.OriginParam.GivenName + "は毒状態になった！";
        owner.GetInterface<ICharaLog>().Log(log);

        var disposable = status.ChangeBarColor(ms_BarColor);
        m_OnFinish.Add(disposable);

        await Task.Delay(500);
    }

    protected override async Task EffectInternal(ICollector owner)
    {
        if (owner.RequireInterface<ICharaAutoRecovery>(out var recovery) == true)
            recovery.Reset();

        if (++m_TurnCount % POISON_DAMAGE_INTERVAL == 0)
        {
            AttackPercentageInfo info = new AttackPercentageInfo(default, default, POISON_DAMAGE_RATIO, 100f, DIRECTION.NONE);

            var turn = owner.GetInterface<ICharaTurn>();
            await turn.WaitFinishActing();
            await owner.GetInterface<ICharaBattle>().DamagePercentage(info);
            await Task.Delay(100);
        }
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