using System.Threading.Tasks;

public class VaccumSlash : Skill
{
    protected override string Name => "しんくうぎり";
    protected override string Description => "遠くの敵を攻撃する。";

    protected override int CoolTime => 10;
    private static readonly float ATK_MAG = 1.5f;
    private static readonly int DISTANCE = 10;

    /// <summary>
    /// 周囲攻撃
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

        var anim = ctx.Owner.GetInterface<ICharaAnimator>();
        var animWait = anim.PlayAnimation(ANIMATION_TYPE.ATTACK, CharaBattle.ms_NormalAttackTotalTime);

        var wait = Task.Delay((int)(CharaBattle.ms_NormalAttackHitTime * 1000));
        await Task.WhenAll(animWait, wait);

        if (ctx.SoundHolder.TryGetSound(CharaSound.ATTACK_SOUND, out var sound) == true)
            sound.Play();

        if (Utility.TryGetForwardUnit(pos, dirV3, DISTANCE, targetType, ctx.DungeonHandler, ctx.UnitFinder, out var target, out var flyDistance) == true)
        {
            var attackInfo = new AttackInfo(ctx.Owner, status.OriginParam.GivenName, (int)(status.Atk * ATK_MAG), CharaBattle.HIT_PROB, CharaBattle.CRITICAL_PROB, false, move.Direction); // 攻撃情報
            var battle = target.GetInterface<ICharaBattle>();
            await battle.Damage(attackInfo);
        }
    }
}
