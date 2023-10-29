using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;

public class FoodAttackUpCondition : Condition
{
    protected override bool CanOverlapping => true;

    private static readonly float ATTACK_UP_MAG = 2f;

    private static readonly float DURING_TIME = 1f;
    private static readonly string ATTACK_UP_FOOD = "AttackUpFood";

    public FoodAttackUpCondition(int remainingTurn) : base(remainingTurn) { }

    protected override void Register(IEffectHolder effectHolder, ISoundHolder soundHolder)
    {

    }

    protected override async Task<bool> OnStart(ICollector owner, IEffectHolder effectHolder, ISoundHolder soundHolder)
    {
        var abnormal = owner.GetInterface<ICharaStatusAbnormality>();
        if (abnormal.IsAttackUpFood == true)
        {
            return false;
        }

        var status = owner.GetInterface<ICharaStatus>();
        string log = status.CurrentStatus.OriginParam.GivenName + "の攻撃力が上がった！";
        owner.GetInterface<ICharaLog>().Log(log);

        // 音
        if (soundHolder.TryGetSound(KeyName.BUFF, out var sound) == true)
            sound.Play();

        // エフェクト
        if (effectHolder.TryGetEffect(ATTACK_UP_FOOD, out var effect) == true)
            m_OnFinish.Add(effect.PlayFollow(owner));

        var buff = status.CurrentStatus.AddBuff(new BuffTicket(PARAMETER_TYPE.ATK, ATTACK_UP_MAG));
        m_OnFinish.Add(buff);
        m_OnFinish.Add(Disposable.CreateWithState(abnormal, abnormal => abnormal.IsAttackUpFood = false));

        abnormal.IsAttackUpFood = true;

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