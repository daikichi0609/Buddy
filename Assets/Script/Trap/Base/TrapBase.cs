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
    Task Effect(TrapSetup trap, ICollector stepper, IUnitFinder unitFinder, AroundCell aroundCell);

    /// <summary>
    /// 罠が見える状態かどうか
    /// </summary>
    bool IsVisible { get; }
}

public abstract class TrapBase : ITrap
{
    private static readonly float UNEXPLODE_RATE = 0.1f;

    private bool m_IsVisible;
    bool ITrap.IsVisible => m_IsVisible;

    async protected virtual Task EffectInternal(ICollector stepper, IUnitFinder unitFinder, AroundCell aroundCell)
    {

    }

    async Task ITrap.Effect(TrapSetup trap, ICollector stepper, IUnitFinder unitFinder, AroundCell aroundCell)
    {
        m_IsVisible = true;

        // ----- ログ ----- //
        var sb = new StringBuilder();
        var charaName = stepper.GetInterface<ICharaStatus>().CurrentStatus.Name;
        sb.Append(charaName + "は" + trap.Type + "を踏んだ！");
        BattleLogManager.Interface.Log(sb.ToString());

        if (ProbabilityCalclator.DetectFromPercent(UNEXPLODE_RATE * 100) == true)
        {
            BattleLogManager.Interface.Log("しかし何も起こらなかった。");
            return;
        }
        // ----- ログ終わり ----- //

        await EffectInternal(stepper, unitFinder, aroundCell);

        var turn = stepper.GetInterface<ICharaTurn>();
        turn.TurnEnd();
    }
}
