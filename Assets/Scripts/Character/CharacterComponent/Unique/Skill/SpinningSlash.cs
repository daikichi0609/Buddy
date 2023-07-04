using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class SpinningSlash : Skill
{
    protected override string Name => "かいてんぎり";
    protected override string Description => "目の前の敵をまとめて攻撃する。";

    protected override int CoolTime => 8;
    private static readonly float ATK_MAG = 1.2f;

    /// <summary>
    /// 周囲攻撃
    /// </summary>
    /// <returns></returns>
    /// <exception cref="System.NotImplementedException"></exception>
    protected override async Task SkillEffect(SkillContext ctx)
    {
        var move = ctx.Owner.GetInterface<ICharaMove>();
        var pos = move.Position;

        var target = ctx.Owner.GetInterface<ICharaTypeHolder>().TargetType;

        var status = ctx.Owner.GetInterface<ICharaStatus>().CurrentStatus;
        ctx.BattleLogManager.Log(status.OriginParam.GivenName + "は" + Name + "を使った！");

        var anim = ctx.Owner.GetInterface<ICharaAnimator>();
        var animWait = anim.PlayAnimation(ANIMATION_TYPE.ATTACK, CharaBattle.ms_NormalAttackTotalTime);

        var wait = Task.Delay((int)(CharaBattle.ms_NormalAttackHitTime * 1000));
        await Task.WhenAll(animWait, wait);

        if (ctx.SoundHolder.TryGetSound(CharaSound.ATTACK_SOUND, out var sound) == true)
            sound.Play();

        var attackInfo = new AttackInfo(ctx.Owner, status.OriginParam.GivenName, (int)(status.Atk * ATK_MAG), CharaBattle.HIT_PROB, CharaBattle.CRITICAL_PROB, false, move.Direction); // 攻撃情報

        var around = ctx.DungeonHandler.GetAroundCell(pos);
        foreach (var cell in around.AroundCells.Values)
        {
            var cellPos = cell.GetInterface<ICellInfoHandler>().Position;
            if (ctx.UnitFinder.TryGetSpecifiedPositionUnit(cellPos, out var unit, target) == true)
            {
                var battle = unit.GetInterface<ICharaBattle>();
                await battle.Damage(attackInfo);
            }
        }
    }
}
