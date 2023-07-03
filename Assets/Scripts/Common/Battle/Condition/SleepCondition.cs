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

    protected override async Task OnStart(ICollector owner)
    {
        var status = owner.GetInterface<ICharaStatus>();
        string log = status.CurrentStatus.OriginParam.GivenName + "は眠ってしまった！";
        owner.GetInterface<ICharaLog>().Log(log);

        var colorChange = status.ChangeBarColor(ms_BarColor);
        m_OnFinish.Add(colorChange);

        var ticket = new FailureTicket<ICollector>(1f, owner =>
             {
                 var status = owner.GetInterface<ICharaStatus>();
                 string log = status.CurrentStatus.OriginParam.GivenName + "は眠っている";
                 owner.GetInterface<ICharaLog>().Log(log);

                 var last = owner.GetInterface<ICharaLastActionHolder>();
                 last.RegisterAction(CHARA_ACTION.WAIT);
             });

        if (owner.RequireInterface<ICharaBattle>(out var battle) == true)
        {
            var disposable = battle.RegisterFailureTicket(ticket);
            m_OnFinish.Add(disposable);
        }

        if (owner.RequireInterface<ICharaMove>(out var move) == true)
        {
            var disposable = move.RegisterFailureTicket(ticket);
            m_OnFinish.Add(disposable);
        }

        if (owner.RequireInterface<ICharaAnimator>(out var anim) == true)
        {
            var disposable = anim.PlayAnimation(ANIMATION_TYPE.SLEEP);
            m_OnFinish.Add(disposable);
        }

        var pos = owner.GetInterface<ICharaMove>().Position;
        await m_EffectHandler.Play(pos, 0.5f);
    }

    protected override async Task EffectInternal(ICollector owner)
    {
        await Task.Delay(500);
    }

    protected override async Task OnFinish(ICollector owner)
    {
        m_OnFinish.Dispose();

        var status = owner.GetInterface<ICharaStatus>();
        string log = status.CurrentStatus.OriginParam.GivenName + "は目を覚ました！";
        owner.GetInterface<ICharaLog>().Log(log);

        await Task.Delay(500);
    }
}
