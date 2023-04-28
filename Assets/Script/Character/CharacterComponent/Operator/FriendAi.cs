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

public partial class FriendAi : CharaAi, IFriendAi
{
    private ReactiveProperty<FRIEND_STATE> m_CurrentState = new ReactiveProperty<FRIEND_STATE>();

    protected override CHARA_TYPE Target => CHARA_TYPE.ENEMY;

    protected override void Register(ICollector owner)
    {
        base.Register(owner);
        owner.Register<IFriendAi>(this);
    }

    /// <summary>
    /// 行動を決めて実行する
    /// </summary>
    public override bool DecideAndExecuteAction()
    {
        var result = false;

        FriendActionClue clue = ConsiderAction(m_CharaMove.Position);
        DIRECTION dir = DIRECTION.NONE;

        switch (clue.State)
        {
            case FRIEND_STATE.ATTACKING:
                dir = LotteryDirection(clue.TargetList);
                result = m_CharaBattle.NormalAttack(dir, Target);
                break;

            case FRIEND_STATE.CHASING:
                // 部屋でPlayerと隣り合ってるかチェック
                bool isNeighborOn = false;
                // 自分とリーダーが同じ部屋にいるなら
                if (DungeonHandler.Interface.TryGetRoomId(m_CharaMove.Position, out var myId) == true &&
                    DungeonHandler.Interface.TryGetRoomId(UnitHolder.Interface.Player.GetInterface<ICharaMove>().Position, out var playerId) &&
                    myId == playerId)
                {
                    var playerPos = UnitHolder.Interface.Player.GetInterface<ICharaMove>().Position;
                    var aroundCell = DungeonHandler.Interface.GetAroundCell(playerPos);
                    foreach (KeyValuePair<DIRECTION, ICollector> pair in aroundCell.Cells)
                    {
                        var info = pair.Value.GetInterface<ICellInfoHolder>();

                        // Unit存在判定
                        if (UnitFinder.Interface.TryGetSpecifiedPositionUnit(info.Position, out var collector, CHARA_TYPE.PLAYER) == false)
                            continue;

                        if (collector == Owner)
                        {
                            isNeighborOn = true; // 隣り合ってるフラグオン
                            dir = pair.Key.ToOppsiteDir(); // プレイヤーの方を向く
                            break;
                        }
                    }
                }

                // 隣り合ってないなら追いかける
                if (isNeighborOn == false)
                    result = Chase(UnitHolder.Interface.Player);
                // 隣り合ってるなら待つ
                else
                {
                    m_CharaMove.Face(dir);
                    result = m_CharaMove.Wait();
                }

                break;
        }

        if (result == true)
        {
            m_CurrentState.Value = clue.State;
            m_CharaTurn.TurnEnd();
        }

        return result;
    }
}

public partial class FriendAi
{
    //味方AI
    private FriendActionClue ConsiderAction(Vector3Int currentPos)
    {
        var aroundCell = DungeonHandler.Interface.GetAroundCell(currentPos);

        // 攻撃対象候補が１つでもあるなら攻撃する
        if (TryGetCandidateAttack(aroundCell, out var attack) == true)
            return new FriendActionClue(FRIEND_STATE.ATTACKING, attack);

        return new FriendActionClue(FRIEND_STATE.CHASING, null);
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