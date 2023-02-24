using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEnemyAi : ICharacterComponent
{
    void DecideAndExecuteAction();
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

public partial class EnemyAi : CharaComponentBase, IEnemyAi
{
    private ICharaMove m_CharaMove;
    private ICharaBattle m_CharaBattle;

    private ENEMY_STATE CurrentState { get; set; }

    private ICell DestinationCell { get; set; }

    private InternalDefine.CHARA_TYPE m_Target = InternalDefine.CHARA_TYPE.PLAYER;

    protected override void Register(ICollector owner)
    {
        base.Register(owner);
        owner.Register<IEnemyAi>(this);
    }

    protected override void Initialize()
    {
        m_CharaMove = Owner.GetComponent<ICharaMove>();
        m_CharaBattle = Owner.GetComponent<ICharaBattle>();
    }

    /// <summary>
    /// 行動を決めて実行する
    /// </summary>
    private void DecideAndExecuteAction()
    {
        ActionClue clue = ConsiderAction(m_CharaMove.Position);
        switch (clue.State)
        {
            case ENEMY_STATE.ATTACKING:
                Face(clue.TargetList);
                NormalAttack();
                break;

            case ENEMY_STATE.CHASING:
                Chase(clue.TargetList);
                break;

            case ENEMY_STATE.SEARCHING:
                SearchPlayer();
                break;
        }

        CurrentState = clue.State;
    }

    void IEnemyAi.DecideAndExecuteAction() => DecideAndExecuteAction();

    /// <summary>
    /// ターゲットの方を向く 主に攻撃前
    /// </summary>
    /// <param name="targetList"></param>
    private ICollector Face(List<ICollector> targets)
    {
        //ターゲットをランダムに絞って向く
        Utility.Shuffle<ICollector>(targets);
        var target = targets[0];
        Vector3 direction = target.GetComponent<ICharaMove>().Position - m_CharaMove.Position;
        m_CharaMove.Face(direction);
        return target;
    }

    private void NormalAttack() => m_CharaBattle.NormalAttack(m_CharaMove.Direction, InternalDefine.CHARA_TYPE.PLAYER);

    /// <summary>
    /// 追いかける
    /// </summary>
    /// <param name="targets"></param>
    private void Chase(List<ICollector> targets)
    {
        Utility.Shuffle<ICollector>(targets);

        List<ICollector> candidates = new List<ICollector>();
        float minDistance = 100f;

        foreach (ICollector candidate in targets)
        {
            var move = candidate.GetComponent<ICharaMove>();
            var distance = (m_CharaMove.Position - move.Position).magnitude;
            if (distance > minDistance)
                continue;
            else if (distance == minDistance)
                candidates.Add(candidate);
            else if(distance < minDistance)
            {
                candidates.Clear();
                candidates.Add(candidate);
            }
        }

        var target = candidates[0];
        var dir = Positional.CalculateDirection(m_CharaMove.Position, target.GetComponent<ICharaMove>().Position);
        if (m_CharaMove.Move(dir) == false)
            m_CharaMove.Wait();
    }

    /// <summary>
    /// プレイヤーを探して歩く
    /// </summary>
    private void SearchPlayer()
    {
        int currentRoomId = DungeonHandler.Interface.GetRoomId(m_CharaMove.Position);
        //通路にいる場合
        if (currentRoomId == 0)
        {
            AroundCellId around = DungeonHandler.Interface.GetAroundCellId((int)m_CharaMove.Position.x, (int)m_CharaMove.Position.z);
            var cells = around.Cells;
            Vector3 lastDirection = m_CharaMove.LastMoveDirection;
            Vector3 oppDirection = lastDirection * -1;
            List<Vector3> directionList = new List<Vector3>();
            if (cells[DIRECTION.UP] > (int)GRID_ID.WALL)
            {
                Vector3 up = new Vector3(0f, 0f, 1f);
                if (lastDirection != up * -1)
                    directionList.Add(up);
            }
            if (cells[DIRECTION.UNDER] > (int)GRID_ID.WALL)
            {
                Vector3 down = new Vector3(0f, 0f, -1f);
                if (lastDirection != down * -1)
                    directionList.Add(down);
            }
            if (cells[DIRECTION.LEFT] > (int)GRID_ID.WALL)
            {
                Vector3 left = new Vector3(-1f, 0f, 0f);
                if (lastDirection != left * -1)
                    directionList.Add(left);
            }
            if (cells[DIRECTION.RIGHT] > (int)GRID_ID.WALL)
            {
                Vector3 right = new Vector3(1f, 0f, 0f);
                if (lastDirection != right * -1)
                    directionList.Add(right);
            }

            Utility.Shuffle(directionList);

            if (m_CharaMove.Move(directionList[0]) == false)
                if (m_CharaMove.Move(oppDirection) == false)
                    m_CharaMove.Wait();
            return;
        }

        //新しくSEARCHINGステートになった場合、目標となる部屋の入り口を設定する
        if (CurrentState != ENEMY_STATE.SEARCHING)
        {
            var gates = DungeonHandler.Interface.GetGateWayCells(DungeonHandler.Interface.GetRoomId(m_CharaMove.Position));

            var candidates = new List<ICell>();
            var minDistance = 999f;
            foreach(ICell cell in gates)
            {
                var distance = (m_CharaMove.Position - cell.Position).magnitude;
                if (distance > minDistance)
                    continue;
                else if (distance == minDistance)
                    candidates.Add(cell);
                else if (distance < minDistance)
                {
                    candidates.Clear();
                    candidates.Add(cell);
                }
            }
            DestinationCell = candidates[0];
        }

        //入り口についた場合、部屋を出る
        if (m_CharaMove.Position.x == DestinationCell.X && m_CharaMove.Position.z == DestinationCell.Z)
        {
            var aroundGridID = DungeonHandler.Interface.GetAroundCellId((int)m_CharaMove.Position.x, (int)m_CharaMove.Position.z);
            var cells = aroundGridID.Cells;
            if (cells[DIRECTION.UP] == (int)GRID_ID.PATH_WAY)
            {
                if (m_CharaMove.Move(new Vector3(0f, 0f, 1f)) == true)
                    return;
            }
            if (cells[DIRECTION.UNDER] == (int)GRID_ID.PATH_WAY)
            {
                if (m_CharaMove.Move(new Vector3(0f, 0f, -1f)) == true)
                    return;
            }
            if (cells[DIRECTION.LEFT] == (int)GRID_ID.PATH_WAY)
            {
                if (m_CharaMove.Move(new Vector3(-1f, 0f, 0f)) == true)
                    return;
            }
            if (cells[DIRECTION.RIGHT] == (int)GRID_ID.PATH_WAY)
            {
                if (m_CharaMove.Move(new Vector3(1f, 0f, 0f)) == true)
                    return;
            }

            // 出られないなら待つ
            m_CharaMove.Wait();
            return;
        }

        //通路出入り口へ向かう
        var dir = Positional.CalculateDirection(m_CharaMove.Position, DestinationCell.Position);
        if (m_CharaMove.Move(dir) == false)
        {
            //移動できなかった場合の処理
            // TODO: 妥協移動の実装
            m_CharaMove.Wait();
        }
    }

    private void CompromiseMove(Vector3 direction) { }
}

public partial class EnemyAi
{
    //敵AI
    private ActionClue ConsiderAction(Vector3 currentPos)
    {
        var aroundCell = DungeonHandler.Interface.GetAroundCell((int)currentPos.x, (int)currentPos.z);

        // 攻撃対象候補が１つでもあるなら攻撃する
        if (TryGetCandidateAttack(aroundCell, m_Target, out var attack) == true)
            return new ActionClue(ENEMY_STATE.ATTACKING, attack);

        if (TryGetCandidateChase(currentPos, m_Target, out var chase) == true)
            return new ActionClue(ENEMY_STATE.CHASING, chase);

        return new ActionClue(ENEMY_STATE.SEARCHING, null);
    }

    private bool TryGetCandidateAttack(AroundCell aroundCell, InternalDefine.CHARA_TYPE target, out List<ICollector> targets)
    {
        targets = new List<ICollector>();

        foreach (KeyValuePair<DIRECTION, ICell> kvp in aroundCell.Cells)
        {
            if (UnitManager.Interface.TryGetSpecifiedPositionUnit(kvp.Value.Position, out var collector, target) == false)
                continue;

            if (DungeonHandler.Interface.CanMoveDiagonal(aroundCell.BaseCell.Position, kvp.Key) == false)
                continue;

            targets.Add(collector);
        }

        return targets.Count != 0;
    }

    private bool TryGetCandidateChase(Vector3 pos, InternalDefine.CHARA_TYPE target, out List<ICollector> targets)
    {
        targets = new List<ICollector>();

        int roomId = DungeonHandler.Interface.GetRoomId(pos);
        if (roomId == 0)
            return false;

        UnitManager.Interface.TryGetSpecifiedRoomUnitList(roomId, out targets, target);

        return targets.Count != 0;
    }
}

public readonly struct ActionClue
{
    public ENEMY_STATE State { get; }
    public List<ICollector> TargetList { get; }

    public ActionClue(ENEMY_STATE state, List<ICollector> targetList)
    {
        State = state;
        TargetList = targetList;
    }
}