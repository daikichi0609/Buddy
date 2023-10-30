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
    SKILL,
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
    protected override async void DecideAndExecuteAction()
    {
        // 眠り状態
        if (await m_CharaAbnormal.Sleep() == true || await m_CharaAbnormal.LostOne() == true)
        {
            await m_CharaTurn.TurnEnd();
            return;
        }

        var result = false;

        var clue = ConsiderAction(m_CharaMove.Position);
        DIRECTION dir = DIRECTION.NONE;

        switch (clue.State)
        {
            case FRIEND_STATE.SKILL:
                dir = clue.TargetDirections.RandomLottery();
                result = await m_CharaSkill.Skill(clue.SkillIndex, dir);
                break;

            case FRIEND_STATE.ATTACKING:
                dir = clue.TargetDirections.RandomLottery();
                result = await m_CharaBattle.NormalAttack(dir);
                break;

            case FRIEND_STATE.CHASING:
                // 同じ部屋でPlayerと隣り合ってるかチェック
                bool isNeighborOn = false;
                var playerPos = m_UnitHolder.Player.GetInterface<ICharaMove>().Position;

                // プレイヤーの周囲セル取得
                var aroundCell = m_DungeonHandler.GetAroundCell(playerPos);
                foreach (KeyValuePair<DIRECTION, ICollector> pair in aroundCell.AroundCells)
                {
                    var info = pair.Value.GetInterface<ICellInfoHandler>();

                    // Unit存在判定
                    if (m_UnitFinder.TryGetSpecifiedPositionUnit(info.Position, out var collector, CHARA_TYPE.FRIEND) == false)
                        continue;

                    // 隣にいるなら
                    if (collector == Owner)
                    {
                        isNeighborOn = true; // 隣り合ってるフラグオン
                        dir = pair.Key.ToOppositeDir(); // プレイヤーへの方向
                        break;
                    }
                }

                // 隣り合っているならプレイヤーを見る
                if (isNeighborOn == true)
                {
                    // 自分とリーダーが同じ部屋にいるなら
                    if (m_DungeonHandler.TryGetRoomId(m_CharaMove.Position, out var myId) == true &&
                        m_DungeonHandler.TryGetRoomId(playerPos, out var playerId) == true &&
                        myId == playerId)
                    {
                        await m_CharaMove.Face(dir);
                        result = m_CharaMove.Wait();
                    }
                    else if (dir.IsDiagonal() == false)
                    {
                        await m_CharaMove.Face(dir);
                        result = m_CharaMove.Wait();
                    }
                    else
                        result = await FollowAstarPath(m_UnitHolder.Player);
                }
                else
                    result = await FollowAstarPath(m_UnitHolder.Player);

                break;
        }

        if (result == true)
        {
            m_CurrentState.Value = clue.State;
            await m_CharaTurn.TurnEnd();
        }
        else
        {
            await Task.Delay(1);
            DecideAndExecuteAction();
        }
    }
}

public partial class FriendAi
{
    //味方AI
    private ActionClue<FRIEND_STATE> ConsiderAction(Vector3Int currentPos)
    {
        if (m_CharaSkill.ShouldUseSkill(out var index, out var dirs) == true)
            return new ActionClue<FRIEND_STATE>(FRIEND_STATE.SKILL, dirs, null, index);

        var aroundCell = m_DungeonHandler.GetAroundCell(currentPos);

        // 攻撃対象候補が１つでもあるなら攻撃する
        if (TryGetCandidateAttack(aroundCell, out var attack) == true)
            return new ActionClue<FRIEND_STATE>(FRIEND_STATE.ATTACKING, attack, null);

        return new ActionClue<FRIEND_STATE>(FRIEND_STATE.CHASING, null, null);
    }
}

public readonly struct ActionClue<T>
{
    /// <summary>
    /// 行動
    /// </summary>
    public T State { get; }

    /// <summary>
    /// 方向
    /// </summary>
    public DIRECTION[] TargetDirections { get; }

    /// <summary>
    /// ターゲット
    /// </summary>
    public ICollector[] TargetUnits { get; }

    /// <summary>
    /// スキルインデックス
    /// </summary>
    public int SkillIndex { get; }

    public ActionClue(T state, DIRECTION[] targetDirections, ICollector[] targetUnits, int skillIndex = -1)
    {
        State = state;
        TargetDirections = targetDirections;
        TargetUnits = targetUnits;
        SkillIndex = skillIndex;
    }
}