using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;

public class Tornade : Skill
{
    protected override string Name => "トルネード";
    protected override string Description => "自分と同じ部屋に居る敵にダメージを与える。";

    protected override int CoolTime => 15;
    private static readonly float ATK_MAG = 1f;

    private static readonly string TORNADE = "Tornade";
    private static readonly float EFFECT_TIME = 0.8f;

    /// <summary>
    /// 遠距離攻撃
    /// </summary>
    /// <returns></returns>
    protected override async Task SkillEffect(SkillContext ctx)
    {
        var move = ctx.Owner.GetInterface<ICharaMove>();
        var pos = move.Position;

        var targetType = ctx.Owner.GetInterface<ICharaTypeHolder>().TargetType;

        var status = ctx.Owner.GetInterface<ICharaStatus>().CurrentStatus;
        ctx.BattleLogManager.Log(status.OriginParam.GivenName + "は" + Name + "を使った！");

        var anim = ctx.Owner.GetInterface<ICharaAnimator>();
        await anim.PlayAnimation(ANIMATION_TYPE.SKILL, 0.5f);

        if (ctx.DungeonHandler.TryGetRoomId(pos, out var roomId) == false || ctx.UnitFinder.TryGetSpecifiedRoomUnitList(roomId, out var targets, targetType) == false)
        {
            ctx.BattleLogManager.Log("しかしうまく決まらなかった！");
            return;
        }

        foreach (var target in targets)
        {
            if (ctx.SoundHolder.TryGetSound(TORNADE, out var sound) == true)
                sound.Play();

            if (ctx.EffectHolder.TryGetEffect(TORNADE, out var effect) == true)
                effect.Play(target);

            await Task.Delay((int)(EFFECT_TIME * 1000));
            var attackInfo = new AttackInfo(ctx.Owner, status.OriginParam.GivenName, (int)(status.Atk * ATK_MAG), CharaBattle.HIT_PROB, CharaBattle.CRITICAL_PROB, false, move.Direction); // 攻撃情報
            var battle = target.GetInterface<ICharaBattle>();
            await battle.Damage(attackInfo);
        }
    }

    protected override bool ExistTarget(SkillTargetContext ctx, out DIRECTION[] dirs)
    {
        List<DIRECTION> list = new List<DIRECTION>();

        if (ctx.DungeonHandler.TryGetRoomId(ctx.Position, out var roomId) == true && ctx.UnitFinder.TryGetSpecifiedRoomUnitList(roomId, out var targets, ctx.TypeHolder.TargetType) == true)
            list.Add(DIRECTION.NONE);

        dirs = list.ToArray();
        return dirs.Length != 0;
    }
}
