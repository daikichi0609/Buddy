using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class SummonKin : Skill
{
    protected override string Name => "眷属召喚";
    protected override string Description => "味方2体を自分の周囲に召喚する";

    protected override int CoolTime => 10;

    private static readonly int KIN_COUNT = 2;
    private static readonly string SUMMON_KIN = "SummonKin";

    [SerializeField]
    private EnemyTableSetup m_EnemyTable;

    /// <summary>
    /// 味方3体を周囲に召喚する
    /// </summary>
    /// <returns></returns>
    /// <exception cref="System.NotImplementedException"></exception>
    protected override async Task SkillEffect(SkillContext ctx)
    {
        var status = ctx.Owner.GetInterface<ICharaStatus>().CurrentStatus;
        ctx.BattleLogManager.Log(status.OriginParam.GivenName + "は" + Name + "を使った！");

        if (ctx.SoundHolder.TryGetSound(SUMMON_KIN, out var sound) == true)
            sound.Play();

        IDisposable disposable = null;
        if (ctx.EffectHolder.TryGetEffect(SUMMON_KIN, out var effect) == true)
            disposable = effect.Play(ctx.Owner);

        var anim = ctx.Owner.GetInterface<ICharaAnimator>();
        await anim.PlayAnimation(ANIMATION_TYPE.SKILL, 0.5f);

        disposable?.Dispose();

        var move = ctx.Owner.GetInterface<ICharaMove>();
        var pos = move.Position;
        var around = ctx.DungeonHandler.GetAroundCell(pos);

        List<Vector3Int> canSummonPos = new List<Vector3Int>();
        foreach (var cell in around.AroundCells.Values)
        {
            var cellHandler = cell.GetInterface<ICellInfoHandler>();
            if (cellHandler.CellId == TERRAIN_ID.INVALID || cellHandler.CellId == TERRAIN_ID.WALL)
                continue;
            if (ctx.UnitFinder.TryGetSpecifiedPositionUnit(cellHandler.Position, out var unit) == true)
                continue;
            canSummonPos.Add(cellHandler.Position);
        }

        var shuffle = canSummonPos.Shuffle();
        for (int i = 0; i < KIN_COUNT; i++)
        {
            if (shuffle.Count <= i)
                break;
            var chara = m_EnemyTable.GetRandomEnemySetup();
            var charaPos = shuffle[i] + new Vector3(0, CharaMove.OFFSET_Y, 0);
            await ctx.DungeonContentsDeployer.DeployEnemy(chara, charaPos);
        }
    }

    protected override bool ExistTarget(SkillTargetContext ctx, out DIRECTION[] dirs)
    {
        List<DIRECTION> list = new List<DIRECTION>
        {
            DIRECTION.NONE
        };
        dirs = list.ToArray();
        return true;
    }
}
