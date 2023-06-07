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
    Task<bool> Effect(TrapSetup trap, ICollector stepper, ICollector cell, IUnitFinder unitFinder, IDungeonHandler dungeonHandler, IBattleLogManager battleLogManager, IEffectHandler effectHandler, Vector3 effectPos);
}

public readonly struct TrapEffectContext
{
    /// <summary>
    /// 罠踏んだ人
    /// </summary>
    public ICollector Owner { get; }

    /// <summary>
    /// 罠があるセル
    /// </summary>
    public ICollector Cell { get; }

    /// <summary>
    /// エフェクト
    /// </summary>
    public IEffectHandler EffectHandler { get; }

    /// <summary>
    /// エフェクト座標
    /// </summary>
    public Vector3 EffectPos { get; }

    /// <summary>
    /// ダンジョン
    /// </summary>
    public IDungeonHandler DungeonHandler { get; }

    /// <summary>
    /// ユニット取得
    /// </summary>
    public IUnitFinder UnitFinder { get; }

    /// <summary>
    /// バトルログ
    /// </summary>
    public IBattleLogManager BattleLogManager { get; }

    public TrapEffectContext(ICollector owner, ICollector cell, IEffectHandler effectHandler, Vector3 effectPos, IDungeonHandler dungeonHandler, IUnitFinder unitFinder, IBattleLogManager battleLogManager)
    {
        Owner = owner;
        Cell = cell;
        EffectHandler = effectHandler;
        EffectPos = effectPos;
        DungeonHandler = dungeonHandler;
        UnitFinder = unitFinder;
        BattleLogManager = battleLogManager;
    }
}

public class TrapEffectBase : ScriptableObject, ITrap
{
    private static readonly float UNEXPLODE_RATE = 0.1f;

    /// <summary>
    /// 罠効果
    /// </summary>
    /// <param name="trap"></param>
    /// <param name="stepper"></param>
    /// <param name="cell"></param>
    /// <param name="unitFinder"></param>
    /// <param name="dungeonHandler"></param>
    /// <param name="battleLogManager"></param>
    /// <param name="effectHandler"></param>
    /// <param name="effectPos"></param>
    /// <returns></returns>
    async Task<bool> ITrap.Effect(TrapSetup trap, ICollector stepper, ICollector cell, IUnitFinder unitFinder, IDungeonHandler dungeonHandler, IBattleLogManager battleLogManager, IEffectHandler effectHandler, Vector3 effectPos)
    {
        // ----- ログ ----- //
        var sb = new StringBuilder();
        var charaName = stepper.GetInterface<ICharaStatus>().CurrentStatus.OriginParam.GivenName;
        sb.Append(charaName + "は" + trap.TrapName + "を踏んだ！");
        battleLogManager.Log(sb.ToString());

        await Task.Delay(500);

        if (ProbabilityCalclator.DetectFromPercent(UNEXPLODE_RATE * 100) == true)
        {
            battleLogManager.Log("しかし何も起こらなかった。");
            return false;
        }
        // ----- ログ終わり ----- //

        await EffectInternal(new TrapEffectContext(stepper, cell, effectHandler, effectPos, dungeonHandler, unitFinder, battleLogManager));
        return true;
    }

    /// <summary>
    /// 罠効果中身
    /// </summary>
    /// <param name="ctx"></param>
    /// <returns></returns>
    protected virtual Task EffectInternal(TrapEffectContext ctx)
    {
        return Task.CompletedTask;
    }
}
