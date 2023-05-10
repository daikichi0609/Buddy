using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class BombTrap : TrapEffectBase
{
    [SerializeField, Header("削る割合")]
    [Range(0f, 1f)]
    private float m_DamageRatio;

    protected override async Task EffectInternal(ICollector stepper, IUnitFinder unitFinder, AroundCell aroundCell, IEffectHandler effect, Vector3 pos)
    {
        await effect.Play(pos);

        // 範囲内の敵に割合ダメージ
        await stepper.GetInterface<ICharaBattle>().DamagePercentage(m_DamageRatio);

        foreach (var cell in aroundCell.Cells.Values)
        {
            var unitPos = cell.GetInterface<ICellInfoHolder>().Position;
            if (unitFinder.TryGetSpecifiedPositionUnit(unitPos, out var unit) == false)
                continue;

            var battle = unit.GetInterface<ICharaBattle>();
            await battle.DamagePercentage(m_DamageRatio);
        }
    }
}
