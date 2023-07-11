using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class PoisonCondition : Condition
{
    protected override bool CanOverlapping => false;

    private static readonly Color32 ms_BarColor = new Color32(167, 87, 168, 255);
    private static readonly int POISON_DAMAGE = 2;
    public static readonly int POISON_DAMAGE_INTERVAL = 3;
    public static readonly int POISON_REMAINING_TURN = 30;
    private static readonly string POISON = "Poison";

    public PoisonCondition(int remainingTurn) : base(remainingTurn) { }

    protected override void Register(IEffectHolder effectHolder, ISoundHolder soundHolder)
    {
        if (effectHolder.TryGetEffect(POISON, out var effect) == true && soundHolder.TryGetSoundObject(POISON, out var sound) == true)
            m_EffectHandler.RegisterEffect(effect, sound);
    }

    protected override async Task OnStart(ICollector owner)
    {
        var status = owner.GetInterface<ICharaStatus>();
        string log = status.CurrentStatus.OriginParam.GivenName + "は毒状態になった！";
        owner.GetInterface<ICharaLog>().Log(log);

        var abnormal = owner.GetInterface<ICharaStatusAbnormality>();
        abnormal.IsPoison = true;

        var disposable = status.ChangeBarColor(ms_BarColor);
        m_OnFinish.Add(disposable);

        var pos = owner.GetInterface<ICharaObjectHolder>().CharaObject.transform.position;
        await m_EffectHandler.Play(pos, 0.5f);
    }

    protected override async Task EffectInternal(ICollector owner)
    {
        if (owner.RequireInterface<ICharaAutoRecovery>(out var recovery) == true)
            recovery.Reset();

        if (m_RemainingTurn % POISON_DAMAGE_INTERVAL == 0)
        {
            AttackFixedInfo info = new AttackFixedInfo(default, default, POISON_DAMAGE, 100f, DIRECTION.NONE);

            var turn = owner.GetInterface<ICharaTurn>();
            await turn.WaitFinishActing();
            var pos = owner.GetInterface<ICharaObjectHolder>().CharaObject.transform.position;
            await m_EffectHandler.Play(pos, 0.5f);
            await owner.GetInterface<ICharaBattle>().Damage(info);
        }
    }

    protected override async Task OnFinish(ICollector owner)
    {
        m_OnFinish.Dispose();

        var status = owner.GetInterface<ICharaStatus>();
        string log = status.CurrentStatus.OriginParam.GivenName + "の毒は治った！";
        owner.GetInterface<ICharaLog>().Log(log);

        var abnormal = owner.GetInterface<ICharaStatusAbnormality>();
        abnormal.IsPoison = false;

        await Task.Delay(500);
    }
}