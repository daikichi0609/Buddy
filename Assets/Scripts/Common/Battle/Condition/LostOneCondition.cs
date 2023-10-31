using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class LostOneCondition : Condition
{
    protected override CONDITION_FINISH_TYPE FinishType => CONDITION_FINISH_TYPE.TURN_START;
    protected override bool CanOverlapping => false;

    private static readonly Color32 ms_BarColor = new Color32(125, 125, 125, 255);
    private static readonly string LOST_ONE = "LostOne";

    public LostOneCondition(int remainingTurn) : base(remainingTurn) { }

    protected override void Register(IEffectHolder effectHolder, ISoundHolder soundHolder)
    {
        if (effectHolder.TryGetEffect(LOST_ONE, out var effect) == true && soundHolder.TryGetSoundObject(LOST_ONE, out var sound) == true)
            m_EffectHandler.RegisterEffect(effect, sound);
    }

    protected override async Task<bool> OnStart(ICollector owner, IEffectHolder effectHolder, ISoundHolder soundHolder)
    {
        var abnormal = owner.GetInterface<ICharaStatusAbnormality>();
        var status = owner.GetInterface<ICharaStatus>();

        if (abnormal.IsLostOne == true)
        {
            string faleLog = status.CurrentStatus.OriginParam.GivenName + "はすでに喪失状態だ。";
            owner.GetInterface<ICharaLog>().Log(faleLog);
            return false;
        }

        string log = status.CurrentStatus.OriginParam.GivenName + "は自我を見失った！";
        owner.GetInterface<ICharaLog>().Log(log);

        if (owner.RequireInterface<ICharaAnimator>(out var anim) == true)
        {
            var disposable = anim.PlayAnimation(ANIMATION_TYPE.SLEEP);
            m_OnFinish.Add(disposable);
        }

        var colorChange = status.ChangeBarColor(ms_BarColor);
        m_OnFinish.Add(colorChange);

        var holder = owner.GetInterface<ICharaObjectHolder>();
        var color = holder.RegisterColor(ms_BarColor);
        m_OnFinish.Add(color);

        var pos = holder.CharaObject.transform.position;
        await m_EffectHandler.Play(pos, 1f);

        abnormal.IsLostOne = true;
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
            string log = status.CurrentStatus.OriginParam.GivenName + "は自我を取り戻した！";
            owner.GetInterface<ICharaLog>().Log(log);
        }

        var abnormal = owner.GetInterface<ICharaStatusAbnormality>();
        abnormal.IsLostOne = false;

        await Task.Delay(500);
    }
}
