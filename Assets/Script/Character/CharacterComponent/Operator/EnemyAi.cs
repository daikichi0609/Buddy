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

public interface IEnemyAi : IAiAction
{
}

/// <summary>
/// 敵の行動タイプ
/// </summary>
public enum ENEMY_STATE
{
    NONE,
    SEARCHING,
    CHASING,
    ATTACKING
}

public partial class EnemyAi : ActorComponentBase, IEnemyAi
{
    private ICharaMove m_CharaMove;
    private ICharaBattle m_CharaBattle;
    private ICharaTurn m_CharaTurn;

    private ReactiveProperty<ENEMY_STATE> m_CurrentState = new ReactiveProperty<ENEMY_STATE>();

    private ICellInfoHolder DestinationCell { get; set; }

    private CHARA_TYPE m_Target = CHARA_TYPE.PLAYER;

    protected override void Register(ICollector owner)
    {
        base.Register(owner);
        owner.Register<IAiAction>(this);
        owner.Register<IEnemyAi>(this);
    }

    protected override void Initialize()
    {
        m_CharaMove = Owner.GetInterface<ICharaMove>();
        m_CharaBattle = Owner.GetInterface<ICharaBattle>();
        m_CharaTurn = Owner.GetInterface<ICharaTurn>();

        // ステート更新で目的地リセット
        m_CurrentState.Subscribe(_ =>
        {
            DestinationCell = null;
        }).AddTo(CompositeDisposable);

        GameManager.Interface.GetUpdateEvent.Subscribe(_ =>
        {
            if (m_CharaTurn.CanAct == true)
                DecideAndExecuteAction();
        });
    }

    /// <summary>
    /// 行動を決めて実行する
    /// </summary>
    public bool DecideAndExecuteAction()
    {
        var result = false;

        EnemyActionClue clue = ConsiderAction(m_CharaMove.Position);

        switch (clue.State)
        {
            case ENEMY_STATE.ATTACKING:
                RandomFace(clue.TargetList);
                result = m_CharaBattle.NormalAttack(m_CharaMove.Direction, m_Target);
                break;

            case ENEMY_STATE.CHASING:
                result = Chase(clue.TargetList);
                break;

            case ENEMY_STATE.SEARCHING:
                result = SearchPlayer();
                break;
        }

        if (result == true)
        {
            m_CurrentState.Value = clue.State;
            m_CharaTurn.TurnEnd();
        }

        return result;
    }

    /// <summary>
    /// ターゲットの方を向く 主に攻撃前
    /// </summary>
    /// <param name="targetList"></param>
    private ICollector RandomFace(List<ICollector> targets)
    {
        //ターゲットをランダムに絞って向く
        targets.Shuffle();
        var target = targets[0];
        var direction = (target.GetInterface<ICharaMove>().Position - m_CharaMove.Position).ToDirEnum();
        m_CharaMove.Face(direction);
        return target;
    }

    /// <summary>
    /// プレイヤーを追いかける
    /// 移動の可否に関わらずtrue
    /// </summary>
    /// <param name="targets"></param>
    /// <returns></returns>
    private bool Chase(List<ICollector> targets)
    {
        targets.Shuffle();
        List<ICollector> candidates = new List<ICollector>();
        float minDistance = 100f;

        foreach (ICollector candidate in targets)
        {
            var move = candidate.GetInterface<ICharaMove>();
            var distance = (m_CharaMove.Position - move.Position).magnitude;
            if (distance > minDistance)
                continue;
            else if (distance == minDistance)
                candidates.Add(candidate);
            else if (distance < minDistance)
            {
                candidates.Clear();
                candidates.Add(candidate);
            }
        }

        var target = candidates[0];
        var dir = Positional.CalculateNormalDirection(m_CharaMove.Position, target.GetInterface<ICharaMove>().Position);

        if (m_CharaMove.Move(dir) == true)
            return true;

        if (CompromiseMove(dir) == true)
            return true;

        return m_CharaMove.Wait();
    }

    /// <summary>
    /// プレイヤーを探して歩く
    /// 移動の可否に関わらずtrue
    /// </summary>
    /// <returns></returns>
    private bool SearchPlayer()
    {
        int currentRoomId = DungeonHandler.Interface.GetRoomId(m_CharaMove.Position);

        //通路にいる場合
        if (currentRoomId == -1)
        {
            AroundCellId around = DungeonHandler.Interface.GetAroundCellId((int)m_CharaMove.Position.x, (int)m_CharaMove.Position.z);
            var cells = around.Cells;
            var lastDirection = m_CharaMove.LastMoveDirection;
            var candidateDir = new List<DIRECTION>();
            var oppDirection = (-1 * lastDirection.ToV3Int()).ToDirEnum();

            if (cells[DIRECTION.UP] > CELL_ID.WALL && DIRECTION.UP != oppDirection)
                candidateDir.Add(DIRECTION.UP);

            if (cells[DIRECTION.UNDER] > CELL_ID.WALL && DIRECTION.UNDER != oppDirection)
                candidateDir.Add(DIRECTION.UNDER);

            if (cells[DIRECTION.LEFT] > CELL_ID.WALL && DIRECTION.LEFT != oppDirection)
                candidateDir.Add(DIRECTION.LEFT);

            if (cells[DIRECTION.RIGHT] > CELL_ID.WALL && DIRECTION.RIGHT != oppDirection)
                candidateDir.Add(DIRECTION.RIGHT);

            if (candidateDir.Count == 0)
                Debug.LogAssertion("行き先候補がない");

            Utility.Shuffle(candidateDir);

            if (m_CharaMove.Move(candidateDir[0]) == true)
                return true;

            if (m_CharaMove.Move(oppDirection) == true)
                return true;

            Debug.Log("移動失敗");
            return m_CharaMove.Wait();
        }

        //新しくSEARCHINGステートになった場合、目標となる部屋の入り口を設定する
        if (DestinationCell == null)
        {
            var roomId = DungeonHandler.Interface.GetRoomId(m_CharaMove.Position);
            var gates = DungeonHandler.Interface.GetGateWayCells(roomId);

            var candidates = new List<ICellInfoHolder>();
            var minDistance = 999f;
            foreach (var cell in gates)
            {
                var info = cell.GetInterface<ICellInfoHolder>();
                var distance = (m_CharaMove.Position - info.Position).magnitude;
                if (distance > minDistance)
                    continue;
                else if (distance == minDistance)
                    candidates.Add(info);
                else if (distance < minDistance)
                {
                    candidates.Clear();
                    candidates.Add(info);
                }
            }
            DestinationCell = candidates[0];
        }

        //入り口についた場合、部屋を出る
        if (m_CharaMove.Position == DestinationCell.Position)
        {
            var aroundGridID = DungeonHandler.Interface.GetAroundCellId((int)m_CharaMove.Position.x, (int)m_CharaMove.Position.z);
            var cells = aroundGridID.Cells;

            // 通路への方向
            var pathDir = DIRECTION.NONE;

            if (cells[DIRECTION.UP] == CELL_ID.PATH_WAY)
                pathDir = DIRECTION.UP;

            if (cells[DIRECTION.UNDER] == CELL_ID.PATH_WAY)
                pathDir = DIRECTION.UNDER;

            if (cells[DIRECTION.LEFT] == CELL_ID.PATH_WAY)
                pathDir = DIRECTION.LEFT;

            if (cells[DIRECTION.RIGHT] == CELL_ID.PATH_WAY)
                pathDir = DIRECTION.RIGHT;

            if (m_CharaMove.Move(pathDir) == true)
            {
                Debug.Log("部屋から出る");
                DestinationCell = null;
                return true;
            }

            // 出られないなら待つ
            return m_CharaMove.Wait();
        }

        //通路出入り口へ向かう
        var dir = Positional.CalculateNormalDirection(m_CharaMove.Position, DestinationCell.Position);
        if (m_CharaMove.Move(dir) == true)
            return true;

        if (CompromiseMove(dir) == true)
            return true;

        return m_CharaMove.Wait();
    }

    /// <summary>
    /// 妥協した移動
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    private bool CompromiseMove(DIRECTION direction)
    {
        var dirs = direction.NearDirection();
        foreach (var dir in dirs)
        {
            if (m_CharaMove.Move(dir) == true)
                return true;
        }
        return false;
    }
}

public partial class EnemyAi
{
    //敵AI
    private EnemyActionClue ConsiderAction(Vector3Int currentPos)
    {
        var aroundCell = DungeonHandler.Interface.GetAroundCell(currentPos);

        // 攻撃対象候補が１つでもあるなら攻撃する
        if (TryGetCandidateAttack(aroundCell, m_Target, out var attack) == true)
            return new EnemyActionClue(ENEMY_STATE.ATTACKING, attack);

        if (TryGetCandidateChase(currentPos, m_Target, out var chase) == true)
            return new EnemyActionClue(ENEMY_STATE.CHASING, chase);

        return new EnemyActionClue(ENEMY_STATE.SEARCHING, null);
    }

    private bool TryGetCandidateAttack(AroundCell aroundCell, CHARA_TYPE target, out List<ICollector> targets)
    {
        targets = new List<ICollector>();
        var baseInfo = aroundCell.BaseCell.GetInterface<ICellInfoHolder>();

        foreach (KeyValuePair<DIRECTION, ICollector> pair in aroundCell.Cells)
        {
            var info = pair.Value.GetInterface<ICellInfoHolder>();

            // Unit存在判定
            if (UnitFinder.Interface.TryGetSpecifiedPositionUnit(info.Position, out var collector, target) == false)
                continue;

            // 壁抜けを判定
            if (DungeonHandler.Interface.CanMove(baseInfo.Position, pair.Key) == false)
                continue;

            targets.Add(collector);
        }

        return targets.Count != 0;
    }

    private bool TryGetCandidateChase(Vector3Int pos, CHARA_TYPE target, out List<ICollector> targets)
    {
        targets = new List<ICollector>();

        int roomId = DungeonHandler.Interface.GetRoomId(pos);
        if (roomId == 0)
            return false;

        UnitFinder.Interface.TryGetSpecifiedRoomUnitList(roomId, out targets, target);

        return targets.Count != 0;
    }
}

public readonly struct EnemyActionClue
{
    public ENEMY_STATE State { get; }
    public List<ICollector> TargetList { get; }

    public EnemyActionClue(ENEMY_STATE state, List<ICollector> targetList)
    {
        State = state;
        TargetList = targetList;
    }
}