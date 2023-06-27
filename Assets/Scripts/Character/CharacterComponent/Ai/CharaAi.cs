using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;
using Zenject;
using static AStarSearch;

public interface IAiAction : IActorInterface
{
    void DecideAndExecuteAction();
}

public abstract partial class CharaAi : ActorComponentBase, IAiAction
{
    [Inject]
    protected IDungeonDeployer m_DungeonDeployer;
    [Inject]
    protected IDungeonHandler m_DungeonHandler;
    [Inject]
    protected IPlayerLoopManager m_LoopManager;
    [Inject]
    protected IUnitHolder m_UnitHolder;
    [Inject]
    protected IUnitFinder m_UnitFinder;

    protected ICharaMove m_CharaMove;
    protected ICharaBattle m_CharaBattle;
    protected ICharaTurn m_CharaTurn;
    protected ICharaTypeHolder m_TypeHolder;

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
        m_TypeHolder = Owner.GetInterface<ICharaTypeHolder>();
    }

    /// <summary>
    /// 行動を決めて実行する
    /// </summary>
    protected abstract void DecideAndExecuteAction();
    void IAiAction.DecideAndExecuteAction() => DecideAndExecuteAction();

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
    /// 移動する
    /// 妥協移動も含める
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    protected bool Move(DIRECTION dir)
    {
        if (m_CharaMove.Move(dir) == false)
            if (CompromiseMove(dir) == false)
                return m_CharaMove.Wait();

        return true;
    }

    /// <summary>
    /// プレイヤーを追いかける
    /// 移動の可否に関わらずtrue
    /// </summary>
    /// <returns></returns>
    protected bool Chase(ICollector target)
    {
        var dir = Positional.CalculateNormalDirection(m_CharaMove.Position, target.GetInterface<ICharaMove>().Position);
        return Move(dir);
    }

    /// <summary>
    /// Astarパスで最初のノードを辿る
    /// </summary>
    protected bool FollowAstarPath(ICollector target)
    {
        var currentPos = m_CharaMove.Position; // 自分の位置
        var targetPos = target.GetInterface<ICharaMove>().Position; // 相手の位置
        var grid = m_DungeonDeployer.CellMap.AstarGrid(); // マップ生成

        // パス生成
        var path = AStarSearch.FindPath(new Vector2Int(currentPos.x, currentPos.z), new Vector2Int(targetPos.x, targetPos.z), grid);
        var first = path[0];
        var firstPos = new Vector3Int(first.X, 0, first.Y);
        var dir = Positional.CalculateNormalDirection(m_CharaMove.Position, firstPos);
        return Move(dir);
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
    protected bool TryGetCandidateAttack(AroundCell<ICollector> aroundCell, out List<ICollector> targets)
    {
        targets = new List<ICollector>();
        var baseInfo = aroundCell.CenterCell.GetInterface<ICellInfoHandler>();

        foreach (KeyValuePair<DIRECTION, ICollector> pair in aroundCell.AroundCells)
        {
            var info = pair.Value.GetInterface<ICellInfoHandler>();

            // Unit存在判定
            if (m_UnitFinder.TryGetSpecifiedPositionUnit(info.Position, out var collector, m_TypeHolder.TargetType) == false)
                continue;

            // 壁抜け判定
            if (m_DungeonHandler.CanMove(baseInfo.Position, pair.Key) == false)
                continue;

            targets.Add(collector);
        }

        return targets.Count != 0;
    }
}