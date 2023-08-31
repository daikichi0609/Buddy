using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class BaleCrow : Skill
{
    protected override string Name => "ベイル・クロー";
    protected override string Description => "前方の敵に攻撃する。";

    protected override int CoolTime => 5;
    private static readonly float ATK_MAG = 2f;

    private static readonly string BALE_CROW = "BaleCrow";

    /// <summary>
    /// 周囲攻撃
    /// </summary>
    /// <returns></returns>
    /// <exception cref="System.NotImplementedException"></exception>
    protected override async Task SkillEffect(SkillContext ctx)
    {
        var status = ctx.Owner.GetInterface<ICharaStatus>().CurrentStatus;
        ctx.BattleLogManager.Log(status.OriginParam.GivenName + "は" + Name + "を使った！");

        if (ctx.SoundHolder.TryGetSound(BALE_CROW, out var sound) == true)
            sound.Play();

        IDisposable disposable = null;
        if (ctx.EffectHolder.TryGetEffect(BALE_CROW, out var effect) == true)
            disposable = effect.Play(ctx.Owner);

        var anim = ctx.Owner.GetInterface<ICharaAnimator>();
        await anim.PlayAnimation(ANIMATION_TYPE.SKILL, 0.5f);

        disposable?.Dispose();

        var move = ctx.Owner.GetInterface<ICharaMove>();
        var attackInfo = new AttackInfo(ctx.Owner, status.OriginParam.GivenName, (int)(status.Atk * ATK_MAG), CharaBattle.HIT_PROB, CharaBattle.CRITICAL_PROB, false, move.Direction); // 攻撃情報
        var attackPos = move.Position + move.Direction.ToV3Int();
        var target = ctx.Owner.GetInterface<ICharaTypeHolder>().TargetType;

        if (ctx.DungeonHandler.CanMove(move.Position, move.Direction) == false ||
        ctx.UnitFinder.TryGetSpecifiedPositionUnit(attackPos, out var collector, target) == false ||
            collector.RequireInterface<ICharaBattle>(out var battle) == false)
        {
            return;
        }
        await battle.Damage(attackInfo);
    }

    protected override bool ExistTarget(SkillTargetContext ctx, out DIRECTION[] dirs)
    {
        List<DIRECTION> list = new List<DIRECTION>();

        foreach (var dir in Positional.Directions)
        {
            if (Positional.TryGetForwardUnit(ctx.Position, dir, 1, ctx.TypeHolder.TargetType, ctx.DungeonHandler, ctx.UnitFinder, out var hit, out var flyDistance) == true)
                list.Add(dir.ToDirEnum());
        }

        dirs = list.ToArray();
        return dirs.Length != 0;
    }
}
