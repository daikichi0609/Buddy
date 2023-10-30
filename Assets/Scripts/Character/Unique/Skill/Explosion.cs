using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;

public class Explosion : Skill
{
    protected override string Name => "エクスプロージョン";
    protected override string Description => "1マス先の敵に大ダメージを与える。その周囲に居る敵にも小ダメージを与える。";

    protected override int CoolTime => 15;
    private static readonly float ATK_MAG_CENTER = 1.5f;
    private static readonly float ATK_MAG_AROUND = 0.5f;
    private static readonly int DISTANCE = 2;

    private static readonly string EXPLOSION = "Explosion";

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

        if (ctx.SoundHolder.TryGetSound(EXPLOSION, out var sound) == true)
            sound.Play();

        IDisposable disposable = null;
        // 攻撃マス
        var targetPos = pos + dirV3 * DISTANCE;

        if (ctx.EffectHolder.TryGetEffect(EXPLOSION, out var effect) == true)
            disposable = effect.Play(targetPos);

        var anim = ctx.Owner.GetInterface<ICharaAnimator>();
        await anim.PlayAnimation(ANIMATION_TYPE.SKILL, 0.5f);

        disposable?.Dispose();

        var attackInfoCenter = new AttackInfo(ctx.Owner, status.OriginParam.GivenName, (int)(status.Atk * ATK_MAG_CENTER), CharaBattle.HIT_PROB, CharaBattle.CRITICAL_PROB, false, move.Direction); // 攻撃情報

        // 攻撃対象ユニットが存在するか調べる
        if (ctx.UnitFinder.TryGetSpecifiedPositionUnit(targetPos, out var hit, targetType) == true)
        {
            var battle = hit.GetInterface<ICharaBattle>();
            await battle.Damage(attackInfoCenter);
        }

        var attackInfoAround = new AttackInfo(ctx.Owner, status.OriginParam.GivenName, (int)(status.Atk * ATK_MAG_AROUND), CharaBattle.HIT_PROB, CharaBattle.CRITICAL_PROB, false, move.Direction); // 攻撃情報
        var around = ctx.DungeonHandler.GetAroundCell(targetPos);
        foreach (var cell in around.AroundCells.Values)
        {
            var cellPos = cell.GetInterface<ICellInfoHandler>().Position;
            if (ctx.UnitFinder.TryGetSpecifiedPositionUnit(cellPos, out var unit, targetType) == true)
            {
                var battle = unit.GetInterface<ICharaBattle>();
                await battle.Damage(attackInfoAround);
            }
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
