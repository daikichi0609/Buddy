using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;

public class Judgement : Skill
{
    protected override string Name => "ジャッジメント";
    protected override string Description => "1マス先の敵にダメージを与える";

    protected override int CoolTime => 10;
    private static readonly float ATK_MAG = 2f;
    private static readonly int DISTANCE = 2;

    private static readonly string JUDGEMENT = "Judgement";
    private static readonly float EFFECT_TIME = 0.8f;
    private static readonly float EFFECT_CHARGE_TIME = 0.7f;

    /// <summary>
    /// 遠距離攻撃
    /// </summary>
    /// <returns></returns>
    protected override async Task SkillEffect(SkillContext ctx)
    {
        var move = ctx.Owner.GetInterface<ICharaMove>();
        var pos = move.Position;
        var dirV3 = move.Direction.ToV3Int();

        var targetType = ctx.Owner.GetInterface<ICharaTypeHolder>().TargetType;

        var status = ctx.Owner.GetInterface<ICharaStatus>().CurrentStatus;
        ctx.BattleLogManager.Log(status.OriginParam.GivenName + "は" + Name + "を使った！");

        var anim = ctx.Owner.GetInterface<ICharaAnimator>();
        await anim.PlayAnimation(ANIMATION_TYPE.SKILL, 0.5f);

        var attackInfo = new AttackInfo(ctx.Owner, status.OriginParam.GivenName, (int)(status.Atk * ATK_MAG), CharaBattle.HIT_PROB, CharaBattle.CRITICAL_PROB, false, move.Direction); // 攻撃情報
        // 攻撃マス
        var targetPos = pos + dirV3 * DISTANCE;

        // 攻撃対象ユニットが存在するか調べる
        if (ctx.UnitFinder.TryGetSpecifiedPositionUnit(targetPos, out var hit, targetType) == true)
        {
            if (ctx.EffectHolder.TryGetEffect(JUDGEMENT, out var effect) == true)
                effect.Play(hit);

            await Task.Delay((int)(EFFECT_CHARGE_TIME * 1000));

            if (ctx.SoundHolder.TryGetSound(JUDGEMENT, out var sound) == true)
                sound.Play();

            await Task.Delay((int)(EFFECT_TIME * 1000));
            var battle = hit.GetInterface<ICharaBattle>();
            await battle.Damage(attackInfo);
        }
    }

    protected override bool ExistTarget(SkillTargetContext ctx, out DIRECTION[] dirs)
    {
        List<DIRECTION> list = new List<DIRECTION>();

        foreach (var dir in Positional.Directions)
        {
            var targetPos = ctx.Position + dir * DISTANCE;
            if (ctx.UnitFinder.TryGetSpecifiedPositionUnit(targetPos, out var unit, ctx.TypeHolder.TargetType) == true)
                list.Add(dir.ToDirEnum());
        }

        dirs = list.ToArray();
        return dirs.Length != 0;
    }
}
