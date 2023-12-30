using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;
using Zenject;

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
    protected ICharaStatusAbnormality m_CharaAbnormal;
    protected ICharaSkillHandler m_CharaSkill;

#if DEBUG
    private List<AStarSearch.Node> m_Path = new List<AStarSearch.Node>();
    public AStarSearch.Node[] Path => m_Path.ToArray();
#endif

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
        m_CharaAbnormal = Owner.GetInterface<ICharaStatusAbnormality>();
        m_CharaSkill = Owner.GetInterface<ICharaSkillHandler>();
    }

    /// <summary>
    /// 行動を決めて実行する
    /// </summary>
    protected abstract void DecideAndExecuteAction();
    void IAiAction.DecideAndExecuteAction()
    {
#if DEBUG
        m_Path.Clear();
#endif 
        DecideAndExecuteAction();
    }

    /// <summary>
    /// 移動する
    /// 妥協移動も含める
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    protected async Task<bool> Move(DIRECTION dir)
    {
        if (await m_CharaMove.Move(dir) == false)
            if (await CompromiseMove(dir) == false)
                return m_CharaMove.Wait();

        return true;
    }

    /// <summary>
    /// プレイヤーを追いかける
    /// 移動の可否に関わらずtrue
    /// </summary>
    /// <returns></returns>
    protected async Task<bool> Chase(ICollector target)
    {
        var dir = Positional.CalculateNormalDirection(m_CharaMove.Position, target.GetInterface<ICharaMove>().Position);
        return await Move(dir);
    }

    /// <summary>
    /// Astarパスで最初のノードを辿る
    /// </summary>
    protected async Task<bool> FollowAstarPath(ICollector target)
    {
        var currentPos = m_CharaMove.Position; // 自分の位置
        var targetPos = target.GetInterface<ICharaMove>().Position; // 相手の位置
        var grid = m_DungeonDeployer.CellMap.AstarGrid(); // マップ生成

        // パス生成
        var path = AStarSearch.FindPath(new Vector2Int(currentPos.x, currentPos.z), new Vector2Int(targetPos.x, targetPos.z), grid);
#if DEBUG
        m_Path = path;
#endif
        // パス取得失敗
        if (path.Count == 0)
        {
#if DEBUG
            Debug.LogError("パス取得失敗");
#endif
            return await Move(DIRECTION.NONE);
        }

        // 次の目標地点
        var first = path[1];
        var firstPos = new Vector3Int(first.X, 0, first.Y);
        var dir = Positional.CalculateNormalDirection(m_CharaMove.Position, firstPos);
        return await Move(dir);
    }

    /// <summary>
    /// 妥協した移動
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    protected async Task<bool> CompromiseMove(DIRECTION direction)
    {
        var dirs = direction.NearDirection();
        foreach (var dir in dirs)
        {
            if (await m_CharaMove.Move(dir) == true)
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
    protected bool TryGetCandidateAttack(AroundCell<ICollector> aroundCell, out DIRECTION[] dirs)
    {
        List<DIRECTION> list = new List<DIRECTION>();
        var baseInfo = aroundCell.CenterCell.GetInterface<ICellInfoHandler>();

        foreach (KeyValuePair<DIRECTION, ICollector> pair in aroundCell.AroundCells)
        {
            var info = pair.Value.GetInterface<ICellInfoHandler>();

            // Unit存在判定
            if (m_UnitFinder.IsUnitOn(info.Position, m_TypeHolder.TargetType) == false)
                continue;

            // 壁抜け判定
            if (m_DungeonHandler.CanMove(baseInfo.Position, pair.Key) == false)
                continue;

            list.Add(pair.Key);
        }

        dirs = list.ToArray();
        return dirs.Length != 0;
    }
}