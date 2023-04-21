using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;

public interface IFriendAi : IAiAction
{
}

/// <summary>
/// 味方の行動タイプ
/// </summary>
public enum FRIEND_STATE
{
    NONE,
    CHASING,
    ATTACKING,
}

public partial class FriendAi : ActorComponentBase, IFriendAi
{
    private ICharaMove m_CharaMove;
    private ICharaBattle m_CharaBattle;
    private ICharaTurn m_CharaTurn;

    private ReactiveProperty<FRIEND_STATE> m_CurrentState = new ReactiveProperty<FRIEND_STATE>();

    private CHARA_TYPE m_Target = CHARA_TYPE.ENEMY;

    protected override void Register(ICollector owner)
    {
        base.Register(owner);
        owner.Register<IAiAction>(this);
        owner.Register<IFriendAi>(this);
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
    public bool DecideAndExecuteAction()
    {
        var result = false;

        FriendActionClue clue = ConsiderAction(m_CharaMove.Position);

        switch (clue.State)
        {
            case FRIEND_STATE.ATTACKING:
                Face(clue.TargetList);
                result = m_CharaBattle.NormalAttack(m_CharaMove.Direction, m_Target);
                break;

            case FRIEND_STATE.CHASING:
                result = Chase();
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
    private ICollector Face(List<ICollector> targets)
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
    /// <returns></returns>
    private bool Chase()
    {
        var target = UnitHolder.Interface.Player;
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

public partial class FriendAi
{
    //味方AI
    private FriendActionClue ConsiderAction(Vector3Int currentPos)
    {
        var aroundCell = DungeonHandler.Interface.GetAroundCell(currentPos);

        // 攻撃対象候補が１つでもあるなら攻撃する
        if (TryGetCandidateAttack(aroundCell, m_Target, out var attack) == true)
            return new FriendActionClue(FRIEND_STATE.ATTACKING, attack);

        return new FriendActionClue(FRIEND_STATE.CHASING, null);
    }

    /// <summary>
    /// 周囲マスに攻撃対象があるなら攻撃する
    /// </summary>
    /// <param name="aroundCell"></param>
    /// <param name="target"></param>
    /// <param name="targets"></param>
    /// <returns></returns>
    private bool TryGetCandidateAttack(AroundCell aroundCell, CHARA_TYPE target, out List<ICollector> targets)
    {
        targets = new List<ICollector>();
        var baseInfo = aroundCell.BaseCell.GetInterface<ICellInfoHolder>();

        foreach (KeyValuePair<DIRECTION, ICollector> pair in aroundCell.Cells)
        {
            var info = pair.Value.GetInterface<ICellInfoHolder>();

            // UNit存在判定
            if (UnitFinder.Interface.TryGetSpecifiedPositionUnit(info.Position, out var collector, target) == false)
                continue;

            // 壁抜け判定
            if (DungeonHandler.Interface.CanMove(baseInfo.Position, pair.Key) == false)
                continue;

            targets.Add(collector);
        }

        return targets.Count != 0;
    }
}

public readonly struct FriendActionClue
{
    public FRIEND_STATE State { get; }
    public List<ICollector> TargetList { get; }

    public FriendActionClue(FRIEND_STATE state, List<ICollector> targetList)
    {
        State = state;
        TargetList = targetList;
    }
}