using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ContinuousSlash : Skill
{
    protected override string Name => "れんぞくぎり";
    protected override string Description => "目の前の敵1体に2-5回の間、連続で攻撃をする。";

    protected override int CoolTime => 6;
    private static readonly float ATK_MAG = 0.8f;

    /// <summary>
    /// 連続攻撃
    /// </summary>
    /// <returns></returns>
    /// <exception cref="System.NotImplementedException"></exception>
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
            var anim = ctx.Owner.GetInterface<ICharaAnimator>();
            var animWait = anim.PlayAnimation(ANIMATION_TYPE.ATTACK, CharaBattle.ms_NormalAttackTotalTime);

            var wait = Task.Delay((int)(CharaBattle.ms_NormalAttackHitTime * 1000));
            await Task.WhenAll(animWait, wait);

            if (ctx.SoundHolder.TryGetSound(CharaSound.ATTACK_SOUND, out var sound) == true)
                sound.Play();

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
}
