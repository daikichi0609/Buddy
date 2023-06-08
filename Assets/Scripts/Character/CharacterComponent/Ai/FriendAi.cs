using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;
using NaughtyAttributes;

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
    [ShowNativeProperty]
    private FRIEND_STATE CurrentState => m_CurrentState.Value;

    protected override void Register(ICollector owner)
    {
        base.Register(owner);
        owner.Register<IFriendAi>(this);
    }

    /// <summary>
    /// 行動を決めて実行する
    /// </summary>
    protected override bool DecideAndExecuteAction()
    {
        var result = false;

        FriendActionClue clue = ConsiderAction(m_CharaMove.Position);
        DIRECTION dir = DIRECTION.NONE;

        switch (clue.State)
        {
            case FRIEND_STATE.ATTACKING:
                dir = LotteryDirection(clue.TargetList);
                result = m_CharaBattle.NormalAttack(dir, m_TypeHolder.TargetType);
                break;

            case FRIEND_STATE.CHASING:
                // 同じ部屋でPlayerと隣り合ってるかチェック
                bool isNeighborOn = false;
                var playerPos = m_UnitHolder.Player.GetInterface<ICharaMove>().Position;
                // 自分とリーダーが同じ部屋にいるなら
                if (m_DungeonHandler.TryGetRoomId(m_CharaMove.Position, out var myId) == true &&
                    m_DungeonHandler.TryGetRoomId(playerPos, out var playerId) == true &&
                    myId == playerId)
                {
                    var aroundCell = m_DungeonHandler.GetAroundCell(playerPos);
                    foreach (KeyValuePair<DIRECTION, ICollector> pair in aroundCell.Cells)
                    {
                        var info = pair.Value.GetInterface<ICellInfoHandler>();

                        // Unit存在判定
                        if (m_UnitFinder.TryGetSpecifiedPositionUnit(info.Position, out var collector, CHARA_TYPE.PLAYER) == false)
                            continue;

                        // 隣にいるなら
                        if (collector == Owner)
                        {
                            isNeighborOn = true; // 隣り合ってるフラグオン
                            dir = pair.Key.ToOppositeDir(); // プレイヤーの方を向く
                            break;
                        }
                    }
                }

                // 隣り合っているならプレイヤーを見る
                if (isNeighborOn == true)
                {
                    m_CharaMove.Face(dir);
                    result = m_CharaMove.Wait();
                }
                // 隣り合っていないなら追いかける AstarPath
                else
                {
                    result = FollowAstarPath(m_UnitHolder.Player);
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
        var aroundCell = m_DungeonHandler.GetAroundCell(currentPos);

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