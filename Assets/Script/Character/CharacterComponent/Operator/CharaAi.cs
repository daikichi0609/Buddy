using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;

public interface IAiAction : IActorInterface
{
    bool DecideAndExecuteAction();
}

public abstract partial class CharaAi : ActorComponentBase, IAiAction
{
    protected ICharaMove m_CharaMove;
    protected ICharaBattle m_CharaBattle;
    protected ICharaTurn m_CharaTurn;

    protected abstract CHARA_TYPE Target { get; }

    protected override void Register(ICollector owner)
    {
        base.Register(owner);
        owner.Register<IAiAction>(this);
    }

    protected override void Initialize()
    {
        m_CharaMove = Owner.GetInterface<ICharaMove>();
        m_CharaBattle = Owner.GetInterface<ICharaBattle>();
        m_CharaTurn = Owner.GetInterface<ICharaTurn>();

        GameManager.Interface.GetUpdateEvent.Subscribe(_ =>
        {
            if (m_CharaTurn.CanAct == true)
                DecideAndExecuteAction();
        });
    }

    /// <summary>
    /// 行動を決めて実行する
    /// </summary>
    public abstract bool DecideAndExecuteAction();

    /// <summary>
    /// ランダムなターゲットへの方向を返す 主に攻撃前
    /// </summary>
    /// <param name="targetList"></param>
    protected DIRECTION LotteryDirection(List<ICollector> targets)
    {
        var target = targets.RandomLottery();
        var direction = (target.GetInterface<ICharaMove>().Position - m_CharaMove.Position).ToDirEnum();
        return direction;
    }

    /// <summary>
    /// プレイヤーを追いかける
    /// 移動の可否に関わらずtrue
    /// </summary>
    /// <returns></returns>
    protected bool Chase(ICollector target)
    {
        var dir = Positional.CalculateNormalDirection(m_CharaMove.Position, target.GetInterface<ICharaMove>().Position);
        if (m_CharaMove.Move(dir) == false)
            if (CompromiseMove(dir) == false)
                m_CharaMove.Wait();

        return true;
    }

    /// <summary>
    /// 妥協した移動
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    protected bool CompromiseMove(DIRECTION direction)
    {
        var dirs = direction.NearDirection();
        foreach (var dir in dirs)
        {
            if (m_CharaMove.Move(dir) == true)
                return true;
        }
        return false;
    }

    /// <summary>
    /// 周囲マスに攻撃対象があるなら攻撃する
    /// </summary>
    /// <param name="aroundCell"></param>
    /// <param name="target"></param>
    /// <param name="targets"></param>
    /// <returns></returns>
    protected bool TryGetCandidateAttack(AroundCell aroundCell, out List<ICollector> targets)
    {
        targets = new List<ICollector>();
        var baseInfo = aroundCell.BaseCell.GetInterface<ICellInfoHolder>();

        foreach (KeyValuePair<DIRECTION, ICollector> pair in aroundCell.Cells)
        {
            var info = pair.Value.GetInterface<ICellInfoHolder>();

            // Unit存在判定
            if (UnitFinder.Interface.TryGetSpecifiedPositionUnit(info.Position, out var collector, Target) == false)
                continue;

            // 壁抜け判定
            if (DungeonHandler.Interface.CanMove(baseInfo.Position, pair.Key) == false)
                continue;

            targets.Add(collector);
        }

        return targets.Count != 0;
    }
}