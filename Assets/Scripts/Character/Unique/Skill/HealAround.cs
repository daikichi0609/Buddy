using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class HealAround : Skill
{
    protected override string Name => "ベリベリヒール";
    protected override string Description => "周囲の味方のHpを回復する。";

    protected override int CoolTime => 7;
    private static readonly float HEAL_MAG = 3f;

    private static readonly string HEAL_BERRY = "HealBerry";

    /// <summary>
    /// 前方範囲攻撃
    /// </summary>
    /// <returns></returns>
    /// <exception cref="System.NotImplementedException"></exception>
    protected override async Task SkillEffect(SkillContext ctx)
    {
        var move = ctx.Owner.GetInterface<ICharaMove>();
        var pos = move.Position;

        var charaStatus = ctx.Owner.GetInterface<ICharaStatus>();
        var status = charaStatus.CurrentStatus;
        ctx.BattleLogManager.Log(status.OriginParam.GivenName + "は" + Name + "を使った！");

        if (ctx.SoundHolder.TryGetSound(HEAL_BERRY, out var sound) == true)
            sound.Play();

        IDisposable disposable = null;
        if (ctx.EffectHolder.TryGetEffect(HEAL_BERRY, out var effect) == true)
            disposable = effect.Play(ctx.Owner);

        var anim = ctx.Owner.GetInterface<ICharaAnimator>();
        await anim.PlayAnimation(ANIMATION_TYPE.SKILL, 0.8f);

        disposable?.Dispose();

        int recover = (int)(status.Atk * HEAL_MAG);
        await charaStatus.RecoverHp(recover);

        var target = ctx.Owner.GetInterface<ICharaTypeHolder>().Type;
        var around = ctx.DungeonHandler.GetAroundCell(pos);

        foreach (var cell in around.AroundCells.Values)
        {
            var cellPos = cell.GetInterface<ICellInfoHandler>().Position;
            if (ctx.UnitFinder.TryGetSpecifiedPositionUnit(cellPos, out var unit, target) == true && unit.RequireInterface<ICharaStatus>(out var unitStatus) == true)
                await unitStatus.RecoverHp(recover);
        }
    }

    protected override bool ExistTarget(SkillTargetContext ctx, out DIRECTION[] dirs)
    {
        List<DIRECTION> list = new List<DIRECTION>();

        foreach (var dir in Positional.Directions)
        {
            if (Positional.TryGetForwardUnit(ctx.Position, dir, 1, ctx.TypeHolder.Type, ctx.DungeonHandler, ctx.UnitFinder, out var hit, out var flyDistance) == true)
                if (hit.RequireInterface<ICharaStatus>(out var status) == true && ctx.Owner.RequireInterface<ICharaStatus>(out var ownerStatus) == true)
                    if (status.CurrentStatus.MaxHp - status.CurrentStatus.Hp >= ownerStatus.CurrentStatus.Atk * HEAL_MAG)
                        list.Add(dir.ToDirEnum());
        }

        dirs = list.ToArray();
        return dirs.Length != 0;
    }
}