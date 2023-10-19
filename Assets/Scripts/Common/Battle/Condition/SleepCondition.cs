using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class SleepCondition : Condition
{
    protected override bool CanOverlapping => false;

    private static readonly Color32 ms_BarColor = new Color32(157, 204, 244, 255);
    private static readonly string SLEEP = "Sleep";

    public SleepCondition(int remainingTurn) : base(remainingTurn) { }

    protected override void Register(IEffectHolder effectHolder, ISoundHolder soundHolder)
    {
        if (effectHolder.TryGetEffect(SLEEP, out var effect) == true && soundHolder.TryGetSoundObject(SLEEP, out var sound) == true)
            m_EffectHandler.RegisterEffect(effect, sound);
    }

    protected override async Task<bool> OnStart(ICollector owner)
    {
        var abnormal = owner.GetInterface<ICharaStatusAbnormality>();
        var status = owner.GetInterface<ICharaStatus>();

        if (abnormal.IsSleeping == true)
        {
            string faleLog = status.CurrentStatus.OriginParam.GivenName + "はすでに眠り状態だ。";
            owner.GetInterface<ICharaLog>().Log(faleLog);
            return false;
        }

        string log = status.CurrentStatus.OriginParam.GivenName + "は眠ってしまった！";
        owner.GetInterface<ICharaLog>().Log(log);

        var colorChange = status.ChangeBarColor(ms_BarColor);
        m_OnFinish.Add(colorChange);

        if (owner.RequireInterface<ICharaAnimator>(out var anim) == true)
        {
            var disposable = anim.PlayAnimation(ANIMATION_TYPE.SLEEP);
            m_OnFinish.Add(disposable);
        }

        var holder = owner.GetInterface<ICharaObjectHolder>();
        var color = holder.RegisterColor(ms_BarColor);
        m_OnFinish.Add(color);

        var pos = holder.CharaObject.transform.position;
        await m_EffectHandler.Play(pos, 1f);

        abnormal.IsSleeping = true;
        return true;
    }

    protected override async Task EffectInternal(ICollector owner)
    {
        var pos = owner.GetInterface<ICharaObjectHolder>().CharaObject.transform.position;
        await m_EffectHandler.Play(pos, 0.5f);
    }

    protected override async Task OnFinish(ICollector owner)
    {
        m_OnFinish.Dispose();

        var status = owner.GetInterface<ICharaStatus>();
        if (status.CurrentStatus.IsDead == false)
        {
            string log = status.CurrentStatus.OriginParam.GivenName + "は目を覚ました！";
            owner.GetInterface<ICharaLog>().Log(log);
        }

        var abnormal = owner.GetInterface<ICharaStatusAbnormality>();
        abnormal.IsSleeping = false;

        await Task.Delay(500);
    }
}
