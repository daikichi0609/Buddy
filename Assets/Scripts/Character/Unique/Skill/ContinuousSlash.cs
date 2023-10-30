using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ContinuousSlash : Skill
{
    protected override string Name => "れんぞくぎり";
    protected override string Description => "目の前の敵1体に2-5回の間、連続で攻撃をする。";

    protected override int CoolTime => 10;
    private static readonly float ATK_MAG = 1.0f;

    /// <summary>
    /// 連続攻撃
    /// </summary>
    /// <returns></returns>
    protected override async Task SkillEffect(SkillContext ctx)
    {
        int attackCount = Random.Range(2, 6);

        var move = ctx.Owner.GetInterface<ICharaMove>();
        var attackPos = move.Position + move.Direction.ToV3Int();

        var target = ctx.Owner.GetInterface<ICharaTypeHolder>().TargetType;

        var status = ctx.Owner.GetInterface<ICharaStatus>().CurrentStatus;
        ctx.BattleLogManager.Log(status.OriginParam.GivenName + "は" + Name + "を使った！");

        while (attackCount > 0)
        {
            if (ctx.SoundHolder.TryGetSound(CharaSound.ATTACK_SOUND, out var sound) == true)
                sound.Play();

            var anim = ctx.Owner.GetInterface<ICharaAnimator>();
            await anim.PlayAnimation(ANIMATION_TYPE.ATTACK, CharaBattle.ms_NormalAttackTotalTime);

            // 角抜け確認
            if (ctx.DungeonHandler.CanMove(move.Position, move.Direction) == false ||
            ctx.UnitFinder.TryGetSpecifiedPositionUnit(attackPos, out var collector, target) == false ||
                collector.RequireInterface<ICharaBattle>(out var battle) == false)
            {
                break;
            }

            var attackInfo = new AttackInfo(ctx.Owner, status.OriginParam.GivenName, (int)(status.Atk * ATK_MAG), CharaBattle.HIT_PROB, CharaBattle.CRITICAL_PROB, false, move.Direction); // 攻撃情報　
            var result = await battle.Damage(attackInfo);
            if (result.IsDead == true)
                break;
            else if (result.IsHit == true)
                attackCount--;
            else
                break;
        }
    }

    protected override bool ExistTarget(SkillTargetContext ctx, out DIRECTION[] dirs)
    {
        List<DIRECTION> list = new List<DIRECTION>();
        var move = ctx.Owner.GetInterface<ICharaMove>();

        foreach (var dir in Positional.Directions)
        {
            var targetPos = ctx.Position + dir;
            if (ctx.UnitFinder.TryGetSpecifiedPositionUnit(targetPos, out var unit, ctx.TypeHolder.TargetType) == true && ctx.DungeonHandler.CanMove(move.Position, move.Direction) == true)
                list.Add(dir.ToDirEnum());
        }

        dirs = list.ToArray();
        return dirs.Length != 0;
    }
}
