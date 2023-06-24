using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class BombTrap : TrapEffectBase
{
    [SerializeField, Header("削る割合")]
    [Range(0f, 1f)]
    private float m_DamageRatio;

    protected override async Task EffectInternal(TrapEffectContext ctx)
    {
        await ctx.EffectHandler.Play(ctx.EffectPos);

        // 範囲内の敵に割合ダメージ
        AttackPercentageInfo info = new AttackPercentageInfo(default, default, m_DamageRatio, 100f, DIRECTION.NONE);
        ctx.Owner.GetInterface<ICharaBattle>().DamagePercentage(info, out var task);
        await task;

        var aroundCell = ctx.Cell.GetAroundCell(ctx.DungeonHandler);
        foreach (var cell in aroundCell.AroundCells.Values)
        {
            var unitPos = cell.GetInterface<ICellInfoHandler>().Position;
            if (ctx.UnitFinder.TryGetSpecifiedPositionUnit(unitPos, out var unit) == false)
                continue;

            var battle = unit.GetInterface<ICharaBattle>();
            battle.DamagePercentage(info, out task);
            await task;
        }
    }
}
