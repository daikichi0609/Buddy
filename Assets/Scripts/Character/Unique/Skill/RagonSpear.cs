using System;
using System.Threading.Tasks;

public class RagonSpear : Skill
{
    protected override string Name => "ラゴン・スピア";
    protected override string Description => "前方2マスまでの敵をまとめて攻撃する。";

    protected override int CoolTime => 20;
    private static readonly float ATK_MAG = 1.3f;
    private static readonly int DISTANCE = 2;

    private static readonly string RAGON_SPEAR = "RagonSpear";

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

        if (ctx.SoundHolder.TryGetSound(RAGON_SPEAR, out var sound) == true)
            sound.Play();

        IDisposable disposable = null;
        if (ctx.EffectHolder.TryGetEffect(RAGON_SPEAR, out var effect) == true)
            disposable = effect.Play(ctx.Owner);

        var anim = ctx.Owner.GetInterface<ICharaAnimator>();
        await anim.PlayAnimation(ANIMATION_TYPE.SKILL, 1f);

        disposable?.Dispose();

        var attackInfo = new AttackInfo(ctx.Owner, status.OriginParam.GivenName, (int)(status.Atk * ATK_MAG), CharaBattle.HIT_PROB, CharaBattle.CRITICAL_PROB, false, move.Direction); // 攻撃情報
        int distance = 1;
        while (distance <= DISTANCE)
        {
            // 攻撃マス
            var targetPos = pos + dirV3 * distance;

            // 攻撃対象ユニットが存在するか調べる
            if (ctx.UnitFinder.TryGetSpecifiedPositionUnit(targetPos, out var hit, targetType) == true)
            {
                var battle = hit.GetInterface<ICharaBattle>();
                await battle.Damage(attackInfo);
            }

            // 地形チェック
            var terrain = ctx.DungeonHandler.GetCellId(targetPos);
            // 壁だったら走査終了
            if (terrain == TERRAIN_ID.WALL)
                break;
        }
    }
}
