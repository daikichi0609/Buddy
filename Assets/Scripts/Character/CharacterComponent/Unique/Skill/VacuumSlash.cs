using System;
using System.Threading.Tasks;

public class VaccumSlash : Skill
{
    protected override string Name => "しんくうぎり";
    protected override string Description => "遠くの敵を攻撃する。";

    protected override int CoolTime => 10;
    private static readonly float ATK_MAG = 1.5f;
    private static readonly int DISTANCE = 10;

    private static readonly string VACCUM_SLASH = "VaccumSlash";

    /// <summary>
    /// 遠距離攻撃
    /// </summary>
    /// <returns></returns>
    /// <exception cref="System.NotImplementedException"></exception>
    protected override async Task SkillEffect(SkillContext ctx)
    {
        var move = ctx.Owner.GetInterface<ICharaMove>();
        var pos = move.Position;
        var dirV3 = move.Direction.ToV3Int();

        var targetType = ctx.Owner.GetInterface<ICharaTypeHolder>().TargetType;

        var status = ctx.Owner.GetInterface<ICharaStatus>().CurrentStatus;
        ctx.BattleLogManager.Log(status.OriginParam.GivenName + "は" + Name + "を使った！");

        if (ctx.SoundHolder.TryGetSound(VACCUM_SLASH, out var sound) == true)
            sound.Play();

        IDisposable disposable = null;
        if (ctx.EffectHolder.TryGetEffect(VACCUM_SLASH, out var effect) == true)
            disposable = effect.Play(ctx.Owner);

        var anim = ctx.Owner.GetInterface<ICharaAnimator>();
        await anim.PlayAnimation(ANIMATION_TYPE.SKILL, CharaBattle.ms_NormalAttackTotalTime);

        disposable?.Dispose();

        if (Utility.TryGetForwardUnit(pos, dirV3, DISTANCE, targetType, ctx.DungeonHandler, ctx.UnitFinder, out var target, out var flyDistance) == true)
        {
            var attackInfo = new AttackInfo(ctx.Owner, status.OriginParam.GivenName, (int)(status.Atk * ATK_MAG), CharaBattle.HIT_PROB, CharaBattle.CRITICAL_PROB, false, move.Direction); // 攻撃情報
            var battle = target.GetInterface<ICharaBattle>();
            await battle.Damage(attackInfo);
        }
    }
}
