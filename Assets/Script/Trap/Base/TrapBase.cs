using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;
using System;

public interface ITrap
{
    /// <summary>
    /// 罠効果
    /// </summary>
    /// <param name="stepper"></param>
    /// <param name="unitFinder"></param>
    /// <returns></returns>
    Task<bool> Effect(TrapSetup trap, ICollector stepper, IUnitFinder unitFinder, AroundCell aroundCell, EffectHadler effect, Vector3 pos);
}

public abstract class TrapBase : ITrap
{
    private static readonly float UNEXPLODE_RATE = 0.1f;

    async protected virtual Task EffectInternal(ICollector stepper, IUnitFinder unitFinder, AroundCell aroundCell, EffectHadler effect, Vector3 pos)
    {

    }

    async Task<bool> ITrap.Effect(TrapSetup trap, ICollector stepper, IUnitFinder unitFinder, AroundCell aroundCell, EffectHadler effect, Vector3 pos)
    {
        // ----- ログ ----- //
        var sb = new StringBuilder();
        var charaName = stepper.GetInterface<ICharaStatus>().CurrentStatus.Name;
        sb.Append(charaName + "は" + trap.Type + "を踏んだ！");
        BattleLogManager.Interface.Log(sb.ToString());

        await Task.Delay(500);

        if (ProbabilityCalclator.DetectFromPercent(UNEXPLODE_RATE * 100) == true)
        {
            BattleLogManager.Interface.Log("しかし何も起こらなかった。");
            return false;
        }
        // ----- ログ終わり ----- //

        await EffectInternal(stepper, unitFinder, aroundCell, effect, pos);
        return true;
    }
}
