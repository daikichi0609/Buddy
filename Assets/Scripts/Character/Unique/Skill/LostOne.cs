using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class LostOne : Skill
{
    protected override string Name => "ロスト・ワン";
    protected override string Description => "目の前の敵を喪失状態にする。";

    protected override int CoolTime => 10;
    private static readonly int LOST_ONE_TURN = 3;

    private static readonly string LOST_ONE_SKILL = "LostOne";
    private static readonly float LOST_ONE_SKILL_TIME = 1f;

    /// <summary>
    /// 喪失状態にする
    /// </summary>
    /// <returns></returns>
    protected override async Task SkillEffect(SkillContext ctx)
    {
        var move = ctx.Owner.GetInterface<ICharaMove>();
        var attackPos = move.Position + move.Direction.ToV3Int();

        var targetType = ctx.Owner.GetInterface<ICharaTypeHolder>().TargetType;

        var status = ctx.Owner.GetInterface<ICharaStatus>().CurrentStatus;
        ctx.BattleLogManager.Log(status.OriginParam.GivenName + "は" + Name + "を使った！");

        var anim = ctx.Owner.GetInterface<ICharaAnimator>();
        await anim.PlayAnimation(ANIMATION_TYPE.SKILL, CharaBattle.ms_NormalAttackTotalTime);

        if (ctx.UnitFinder.TryGetSpecifiedPositionUnit(attackPos, out var target, targetType) == false)
            return;

        if (ctx.SoundHolder.TryGetSound(LOST_ONE_SKILL, out var sound) == true)
            sound.Play();

        if (ctx.EffectHolder.TryGetEffect(LOST_ONE_SKILL, out var effect) == true)
            effect.Play(target);

        await Task.Delay((int)(LOST_ONE_SKILL_TIME * 1000));

        //  喪失状態にする
        var condition = target.GetInterface<ICharaCondition>();
        await condition.AddCondition(new LostOneCondition(LOST_ONE_TURN));
    }

    protected override bool ExistTarget(SkillTargetContext ctx, out DIRECTION[] dirs)
    {
        List<DIRECTION> list = new List<DIRECTION>();

        foreach (var dir in Positional.Directions)
        {
            var targetPos = ctx.Position + dir;
            if (ctx.UnitFinder.TryGetSpecifiedPositionUnit(targetPos, out var unit, ctx.TypeHolder.TargetType) == true)
                list.Add(dir.ToDirEnum());
        }

        dirs = list.ToArray();
        return dirs.Length != 0;
    }
}
