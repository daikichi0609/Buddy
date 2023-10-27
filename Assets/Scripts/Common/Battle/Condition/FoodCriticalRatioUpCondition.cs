using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;

public class FoodCriticalRatioUpCondition : Condition
{
    protected override bool CanOverlapping => true;

    private static readonly float CRITICAL_RATIO_MAG = 3f;

    private static readonly float DURING_TIME = 1f;
    private static readonly string CRITICAL_RATIO_UP_FOOD = "CriticalRatioUpFood";

    public FoodCriticalRatioUpCondition(int remainingTurn) : base(remainingTurn) { }

    protected override void Register(IEffectHolder effectHolder, ISoundHolder soundHolder)
    {

    }

    protected override async Task<bool> OnStart(ICollector owner, IEffectHolder effectHolder, ISoundHolder soundHolder)
    {
        var abnormal = owner.GetInterface<ICharaStatusAbnormality>();
        if (abnormal.IsCriticalRatioUpFood == true)
        {
            return false;
        }

        var status = owner.GetInterface<ICharaStatus>();
        string log = status.CurrentStatus.OriginParam.GivenName + "のクリティカル率が上がった！";
        owner.GetInterface<ICharaLog>().Log(log);

        // 音
        if (soundHolder.TryGetSound(KeyName.BUFF, out var sound) == true)
            sound.Play();

        // エフェクト
        if (effectHolder.TryGetEffect(CRITICAL_RATIO_UP_FOOD, out var effect) == true)
            m_OnFinish.Add(effect.PlayFollow(owner));

        var buff = status.CurrentStatus.AddBuff(new BuffTicket(PARAMETER_TYPE.CR, CRITICAL_RATIO_MAG));
        m_OnFinish.Add(buff);
        m_OnFinish.Add(Disposable.CreateWithState(abnormal, abnormal => abnormal.IsCriticalRatioUpFood = false));

        abnormal.IsCriticalRatioUpFood = true;

        await Task.Delay((int)(DURING_TIME * 1000));
        return true;
    }

    protected override Task EffectInternal(ICollector owner)
    {
        return Task.CompletedTask;
    }

    protected override Task OnFinish(ICollector owner)
    {
        m_OnFinish.Dispose();
        return Task.CompletedTask;
    }
}