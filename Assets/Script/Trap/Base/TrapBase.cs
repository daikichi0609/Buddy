using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public interface ITrap
{
    /// <summary>
    /// 罠タイプ
    /// </summary>
    TRAP_TYPE TrapType { get; }

    /// <summary>
    /// 罠効果
    /// </summary>
    /// <param name="stepper"></param>
    /// <param name="unitFinder"></param>
    /// <returns></returns>
    Task Effect(ICollector stepper, IUnitFinder unitFinder, AroundCell aroundCell);
}

public abstract class TrapBase : ITrap
{
    protected abstract TRAP_TYPE TrapType { get; }
    TRAP_TYPE ITrap.TrapType => TrapType;

    private static readonly float UNEXPLODE_RATE = 0.1f;

    async protected virtual Task EffectInternal(ICollector stepper, IUnitFinder unitFinder, AroundCell aroundCell)
    {

    }

    async Task ITrap.Effect(ICollector stepper, IUnitFinder unitFinder, AroundCell aroundCell)
    {
        var sb = new StringBuilder();
        var charaName = stepper.GetInterface<ICharaStatus>().CurrentStatus.Name;
        sb.Append(charaName + "は" + TrapType + "を踏んだ！");
        BattleLogManager.Interface.Log(sb.ToString());

        if (ProbabilityCalclator.DetectFromPercent(UNEXPLODE_RATE * 100) == true)
        {
            BattleLogManager.Interface.Log("しかし何も起こらなかった。");
            return;
        }

        await EffectInternal(stepper, unitFinder, aroundCell);
    }

    private void PostEffect()
    {

    }
}
