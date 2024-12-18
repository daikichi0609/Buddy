﻿using UnityEngine;
using UniRx;
using System;
using NaughtyAttributes;
using Zenject;
using System.Threading.Tasks;
using System.Collections.Generic;

public interface ICharaMove : IActorInterface
{
    /// <summary>
    /// 座標
    /// </summary>
    Vector3Int Position { get; }

    /// <summary>
    /// 向き
    /// </summary>
    DIRECTION Direction { get; }

    /// <summary>
    /// 最後に移動した方向
    /// </summary>
    DIRECTION LastMoveDirection { get; }

    /// <summary>
    /// 入れ替わりをするときの待ち合わせ情報
    /// </summary>
    CharaMove.MoveSwitchInfo SwitchInfo { get; set; }

    /// <summary>
    /// 向き直る
    /// </summary>
    /// <param name="direction"></param>
    Task Face(DIRECTION direction);

    /// <summary>
    /// 敵に向き直る
    /// </summary>
    Task FaceToEnemy();

    /// <summary>
    /// 移動
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    Task<bool> Move(DIRECTION dir);

    /// <summary>
    /// 強制的に移動
    /// </summary>
    /// <param name="dir"></param>
    Task ForcedMove(DIRECTION dir);

    /// <summary>
    /// 待機
    /// </summary>
    /// <returns></returns>
    bool Wait();

    /// <summary>
    /// ワープ
    /// </summary>
    /// <param name="pos"></param>
    void Warp(Vector3Int pos);

    /// <summary>
    /// 移動しているか
    /// </summary>
    bool IsMoving { get; }
}

public interface ICharaMoveEvent : IActorEvent
{
    IObservable<Unit> OnMoveStart { get; }
    IObservable<Unit> OnMoveEnd { get; }
}

public class CharaMove : ActorComponentBase, ICharaMove, ICharaMoveEvent
{
    [Inject]
    private IDungeonHandler m_DungeonHandler;
    [Inject]
    private IUnitFinder m_UnitFinder;

    private ICharaObjectHolder m_ObjectHolder;
    private GameObject CharaObject => m_ObjectHolder.CharaObject;
    private GameObject MoveObject => m_ObjectHolder.MoveObject;
    private ICharaTypeHolder m_Type;
    private ICharaLastActionHolder m_CharaLastActionHolder;
    private ICharaTurn m_CharaTurn;
    private ICharaTurnEvent m_CharaTurnEvent;
    private ICharaAnimator m_CharaAnimator;
    private ICharaStatusAbnormality m_Abnormal;

    /// <summary>
    /// 移動前に呼ばれる
    /// </summary>
    private Subject<Unit> m_OnMoveStart = new Subject<Unit>();
    IObservable<Unit> ICharaMoveEvent.OnMoveStart => m_OnMoveStart;

    /// <summary>
    /// 移動後に呼ばれる
    /// </summary>
    private Subject<Unit> m_OnMoveEnd = new Subject<Unit>();
    IObservable<Unit> ICharaMoveEvent.OnMoveEnd => m_OnMoveEnd;

    private static readonly float SPEED_MAG = 3f;
    public static readonly float OFFSET_Y = 0.51f;

    /// <summary>
    /// 位置
    /// </summary>
    [ShowNativeProperty]
    private Vector3Int Position { get; set; }
    Vector3Int ICharaMove.Position => Position;

    /// <summary>
    /// 向いている方向
    /// </summary>
    private DIRECTION Direction { get; set; }
    DIRECTION ICharaMove.Direction => Direction;

    /// <summary>
    /// 前回の移動した方向
    /// </summary>
    private DIRECTION LastMoveDirection { get; set; }
    DIRECTION ICharaMove.LastMoveDirection => LastMoveDirection;

    /// <summary>
    /// 現在の移動タスク
    /// </summary>
    private Task m_MovingTask;

    /// <summary>
    /// 移動中かどうか
    /// </summary>
    private bool IsMoving
    {
        get
        {
            if (m_MovingTask != null && m_MovingTask.IsCompleted == false)
                return true;

            return false;
        }
    }
    bool ICharaMove.IsMoving => IsMoving;

    /// <summary>
    /// 入れ替わるユニット
    /// </summary>
    private MoveSwitchInfo m_SwitchInfo;
    MoveSwitchInfo ICharaMove.SwitchInfo { get => m_SwitchInfo; set => m_SwitchInfo = value; }

    public sealed class MoveSwitchInfo
    {
        public ICollector Switcher { get; }
        public Task SwitchTask { get; }

        public MoveSwitchInfo(ICollector switcher, Task task)
        {
            Switcher = switcher;
            SwitchTask = task;
        }
    }

    /// <summary>
    /// 敵と隣接している方向
    /// </summary>
    private List<DIRECTION> m_EnemyDir;
    private int m_EnemyDirIndex;

    protected override void Register(ICollector owner)
    {
        base.Register(owner);
        owner.Register<ICharaMove>(this);
        owner.Register<ICharaMoveEvent>(this);
    }

    protected override void Initialize()
    {
        m_ObjectHolder = Owner.GetInterface<ICharaObjectHolder>();
        m_Type = Owner.GetInterface<ICharaTypeHolder>();
        m_CharaLastActionHolder = Owner.GetInterface<ICharaLastActionHolder>();
        m_CharaTurn = Owner.GetInterface<ICharaTurn>();
        m_CharaTurnEvent = Owner.GetEvent<ICharaTurnEvent>();
        m_CharaAnimator = Owner.GetInterface<ICharaAnimator>();
        m_Abnormal = Owner.GetInterface<ICharaStatusAbnormality>();

        // ----- 初期化 ----- //
        Direction = DIRECTION.UNDER;
        LastMoveDirection = DIRECTION.UNDER;
        var pos = MoveObject.transform.position;
        Position = new Vector3Int((int)pos.x, 0, (int)pos.z);

        // アクション登録
        m_OnMoveStart.SubscribeWithState(this, (_, self) => self.m_CharaLastActionHolder.RegisterAction(CHARA_ACTION.MOVE)).AddTo(Owner.Disposables);

        // 敵振り向き
        m_CharaTurnEvent.OnTurnEnd.SubscribeWithState(this, (_, self) =>
        {
            self.m_EnemyDir = null;
            self.m_EnemyDirIndex = 0;
        }).AddTo(Owner.Disposables);
    }

    /// <summary>
    /// 向きを変える
    /// </summary>
    /// <param name="direction"></param>
    private Task Face(DIRECTION direction)
    {
        if (direction == DIRECTION.NONE)
            return Task.CompletedTask;

        if (m_Abnormal.IsSleeping == true || m_Abnormal.IsLostOne == true)
            return Task.CompletedTask;

        Direction = direction;
        CharaObject.transform.rotation = Quaternion.LookRotation(direction.ToV3Int());
        return Task.CompletedTask;
    }
    Task ICharaMove.Face(DIRECTION direction) => Face(direction);

    /// <summary>
    /// 移動
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    async Task<bool> ICharaMove.Move(DIRECTION direction)
    {
        // 前の移動タスクを待つ
        await WaitMovingTaskComplete();

        //向きを変える
        await Face(direction);

        //壁抜けはできない
        if (m_DungeonHandler.CanMove(Position, direction) == false)
            return false;

        Vector3Int destinationPos = Position + direction.ToV3Int();

        // 他ユニットがいる場合の入れ違い処理
        if (m_UnitFinder.TryGetSpecifiedPositionUnit(destinationPos, out var unit) == true)
        {
            // プレイヤーキャラでないなら、もしくは相手がプレイヤーキャラでないなら入れ違わない
            if (m_Type.Type != CHARA_TYPE.FRIEND || unit.GetInterface<ICharaTypeHolder>().Type != CHARA_TYPE.FRIEND || unit.GetInterface<ICharaStatusAbnormality>().IsSleeping == true)
                return false;
            else
            {
                var move = unit.GetInterface<ICharaMove>();
                if (move.IsMoving == true)
                    return false;

                var last = unit.GetInterface<ICharaLastActionHolder>();
                // ターン消費していないなら入れ違い
                if (last.LastAction != CHARA_ACTION.NONE)
                    return false; // 入れ違いできないなら移動不可

                Debug.Log(last.LastAction);
                var forced = move.ForcedMove(direction.ToOppositeDir());
                m_SwitchInfo = new MoveSwitchInfo(unit, forced);
            }
        }

        // awaitしない
        var _ = MoveInternal(destinationPos, direction);
        return true;
    }

    /// <summary>
    /// 移動呼び出し
    /// </summary>
    /// <param name="dest"></param>
    /// <param name="dir"></param>
    private async Task MoveInternal(Vector3Int dest, DIRECTION dir)
    {
        // 座標設定
        var destPos = dest + new Vector3(0f, OFFSET_Y, 0f);
        LastMoveDirection = dir;

        // 内部的には先に移動しとく
        Position = dest;

        // 移動開始イベント
        m_OnMoveStart.OnNext(Unit.Default);

        var animation = m_CharaAnimator.PlayAnimation(ANIMATION_TYPE.MOVE);
        var acting = m_CharaTurn.RegisterActing();

        // 移動タスク
        float speedMag = dir.IsDiagonal() ? 1.4f : 1f;
        m_MovingTask = MoveTask(destPos, speedMag);
        await m_MovingTask;

        animation.Dispose();
        acting.Dispose();

        m_OnMoveEnd.OnNext(Unit.Default);
    }

    /// <summary>
    /// 移動処理
    /// </summary>
    /// <param name="dest"></param>
    /// <returns></returns>
    private async Task MoveTask(Vector3 dest, float speedMag)
    {
        while ((MoveObject.transform.position - dest).magnitude > 0.01f)
        {
            MoveObject.transform.position = Vector3.MoveTowards(MoveObject.transform.position, dest, Time.deltaTime * speedMag * SPEED_MAG);
            await Task.Delay(1);
        }
    }

    /// <summary>
    /// 待機 ターン終了するだけ
    /// </summary>
    bool ICharaMove.Wait()
    {
        m_CharaLastActionHolder.RegisterAction(CHARA_ACTION.WAIT);
        return true;
    }

    /// <summary>
    /// ワープ
    /// </summary>
    /// <param name="pos"></param>
    void ICharaMove.Warp(Vector3Int pos)
    {
        Position = pos;
        var v3 = new Vector3(Position.x, OFFSET_Y, Position.z);
        m_ObjectHolder.MoveObject.transform.position = v3;
    }

    /// <summary>
    /// 強制的に移動
    /// 入れ替わり用
    /// </summary>
    /// <param name="dir"></param>
    async Task ICharaMove.ForcedMove(DIRECTION dir)
    {
        await m_CharaTurn.WaitFinishActing(); // 周りを待つ
        await WaitMovingTaskComplete(); // 自分の移動完了を待つ

        await Face(dir);
        Vector3Int destinationPos = Position + dir.ToV3Int();
        await MoveInternal(destinationPos, dir);
    }

    /// <summary>
    /// 前の移動タスクを待つ
    /// </summary>
    /// <returns></returns>
    private async Task WaitMovingTaskComplete()
    {
        // 前の移動タスクの完了を待つ
        while (IsMoving == true)
            await Task.Delay(1);
    }

    /// <summary>
    /// 周囲の敵に向き直る
    /// </summary>
    /// <returns></returns>
    async Task ICharaMove.FaceToEnemy()
    {
        if (m_EnemyDir == null)
        {
            m_EnemyDir = new List<DIRECTION>();
            // 敵と隣接している方向を探る
            foreach (var dir in Positional.Directions)
            {
                if (Positional.TryGetForwardUnit(Position, dir, 1, m_Type.TargetType, m_DungeonHandler, m_UnitFinder, out var hit, out var flyDistance) == true)
                    m_EnemyDir.Add(dir.ToDirEnum());
            }
        }

        // リスト空ならなにもしない
        if (m_EnemyDir.Count == 0)
            return;

        // インクリメントして敵の方向向く
        if (++m_EnemyDirIndex >= m_EnemyDir.Count)
            m_EnemyDirIndex = 0;
        await Face(m_EnemyDir[m_EnemyDirIndex]);
    }
}