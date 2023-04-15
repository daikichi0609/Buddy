using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class BombTrap : TrapBase
{
    private static readonly float DAMAGE_RATIO = 0.5f;

    protected override async Task EffectInternal(ICollector stepper, IUnitFinder unitFinder, AroundCell aroundCell, EffectHadler effect, Vector3 pos)
    {
        effect.Play(pos);

        // 範囲内の敵に割合ダメージ
        await stepper.GetInterface<ICharaBattle>().DamagePercentage(DAMAGE_RATIO);

        foreach (var cell in aroundCell.Cells.Values)
        {
            var unitPos = cell.GetInterface<ICellInfoHolder>().Position;
            if (unitFinder.TryGetSpecifiedPositionUnit(unitPos, out var unit) == false)
                continue;

            var battle = unit.GetInterface<ICharaBattle>();
            await battle.DamagePercentage(DAMAGE_RATIO);
        }
    }
}
