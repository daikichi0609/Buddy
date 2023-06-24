using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;
using NaughtyAttributes;
using Zenject;

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

public partial class EnemyAi : CharaAi, IEnemyAi
{
    private ReactiveProperty<ENEMY_STATE> m_CurrentState = new ReactiveProperty<ENEMY_STATE>();
    [ShowNativeProperty]
    private ENEMY_STATE CurrentState => m_CurrentState.Value;

    [ShowNativeProperty]
    private ICellInfoHandler DestinationCell { get; set; }

    protected override void Register(ICollector owner)
    {
        base.Register(owner);
        owner.Register<IEnemyAi>(this);
    }

    protected override void Initialize()
    {
        base.Initialize();

        // ステート更新で目的地リセット
        m_CurrentState.SubscribeWithState(this, (_, self) =>
        {
            self.DestinationCell = null;
        }).AddTo(Owner.Disposables);
    }

    /// <summary>
    /// 行動を決めて実行する
    /// </summary>
    protected override bool DecideAndExecuteAction()
    {
        var result = false;

        EnemyActionClue clue = ConsiderAction(m_CharaMove.Position);
        DIRECTION dir = DIRECTION.NONE;

        switch (clue.State)
        {
            case ENEMY_STATE.ATTACKING:
                dir = LotteryDirection(clue.TargetList);
                result = m_CharaBattle.NormalAttack(dir, m_TypeHolder.TargetType);
                break;

            case ENEMY_STATE.CHASING:
                // 1番距離の近いキャラに近づく
                List<ICollector> candidates = new List<ICollector>();
                float minDistance = 100f;

                foreach (ICollector candidate in clue.TargetList)
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

                // 抽選完了
                var target = candidates[0];
                result = Chase(target);
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
    /// プレイヤーを探して歩く
    /// 移動の可否に関わらずtrue
    /// </summary>
    /// <returns></returns>
    private bool SearchPlayer()
    {
        //通路にいる場合
        if (m_DungeonHandler.TryGetRoomId(m_CharaMove.Position, out var roomId) == false)
        {
            var around = m_DungeonHandler.GetAroundCellId(m_CharaMove.Position);
            var cells = around.AroundCells;
            var lastDirection = m_CharaMove.LastMoveDirection;
            var candidateDir = new List<DIRECTION>();
            var oppDirection = (-1 * lastDirection.ToV3Int()).ToDirEnum();

            if (cells[DIRECTION.UP] > TERRAIN_ID.WALL && DIRECTION.UP != oppDirection)
                candidateDir.Add(DIRECTION.UP);

            if (cells[DIRECTION.UNDER] > TERRAIN_ID.WALL && DIRECTION.UNDER != oppDirection)
                candidateDir.Add(DIRECTION.UNDER);

            if (cells[DIRECTION.LEFT] > TERRAIN_ID.WALL && DIRECTION.LEFT != oppDirection)
                candidateDir.Add(DIRECTION.LEFT);

            if (cells[DIRECTION.RIGHT] > TERRAIN_ID.WALL && DIRECTION.RIGHT != oppDirection)
                candidateDir.Add(DIRECTION.RIGHT);

#if DEBUG
            if (candidateDir.Count == 0)
                Debug.LogAssertion("行き先候補がない");
#endif

            Utility.RandomLottery(candidateDir);

            if (m_CharaMove.Move(candidateDir[0]) == true)
                return true;

            if (m_CharaMove.Move(oppDirection) == true)
                return true;

#if DEBUG
            Debug.Log("移動失敗");
#endif
            return m_CharaMove.Wait();
        }

        //新しくSEARCHINGステートになった場合、目標となる部屋の入り口を設定する
        if (DestinationCell == null)
        {
            var gates = m_DungeonHandler.GetGateWayCells(roomId);

            var candidates = new List<ICellInfoHandler>();
            var minDistance = 999f;
            foreach (var cell in gates)
            {
                var info = cell.GetInterface<ICellInfoHandler>();
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
            var aroundGridID = m_DungeonHandler.GetAroundCellId(m_CharaMove.Position);
            var cells = aroundGridID.AroundCells;

            // 通路への方向
            var pathDir = DIRECTION.NONE;

            if (cells[DIRECTION.UP] == TERRAIN_ID.PATH_WAY)
                pathDir = DIRECTION.UP;

            if (cells[DIRECTION.UNDER] == TERRAIN_ID.PATH_WAY)
                pathDir = DIRECTION.UNDER;

            if (cells[DIRECTION.LEFT] == TERRAIN_ID.PATH_WAY)
                pathDir = DIRECTION.LEFT;

            if (cells[DIRECTION.RIGHT] == TERRAIN_ID.PATH_WAY)
                pathDir = DIRECTION.RIGHT;

            if (m_CharaMove.Move(pathDir) == true)
            {
#if DEBUG
                Debug.Log("部屋から出る");
#endif
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
}

public partial class EnemyAi
{
    //敵AI
    private EnemyActionClue ConsiderAction(Vector3Int currentPos)
    {
        var aroundCell = m_DungeonHandler.GetAroundCell(currentPos);

        // 攻撃対象候補が１つでもあるなら攻撃する
        if (TryGetCandidateAttack(aroundCell, out var attack) == true)
            return new EnemyActionClue(ENEMY_STATE.ATTACKING, attack);

        if (TryGetCandidateChase(currentPos, m_TypeHolder.TargetType, out var chase) == true)
            return new EnemyActionClue(ENEMY_STATE.CHASING, chase);

        return new EnemyActionClue(ENEMY_STATE.SEARCHING, null);
    }

    private bool TryGetCandidateChase(Vector3Int pos, CHARA_TYPE target, out List<ICollector> targets)
    {
        targets = new List<ICollector>();

        if (m_DungeonHandler.TryGetRoomId(pos, out var roomId) == false)
            return false;

        return m_UnitFinder.TryGetSpecifiedRoomUnitList(roomId, out targets, target);
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