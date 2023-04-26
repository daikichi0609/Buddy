﻿using UnityEngine;
using UniRx;
using System;
using NaughtyAttributes;

public interface ICharaMove : IActorInterface
{
    Vector3Int Position { get; }
    DIRECTION Direction { get; }
    DIRECTION LastMoveDirection { get; }

    void Face(DIRECTION direction);
    bool Move(DIRECTION dir);
    void ForcedMove(DIRECTION dir);
    bool Wait();

    void Warp(Vector3Int pos);
}

public interface ICharaMoveEvent : IActorEvent
{
    IObservable<Unit> OnMoveStart { get; }
    IObservable<Unit> OnMoveEnd { get; }
}

public class CharaMove : ActorComponentBase, ICharaMove, ICharaMoveEvent
{
    private ICharaObjectHolder m_ObjectHolder;
    private GameObject CharaObject => m_ObjectHolder.CharaObject;
    private GameObject MoveObject => m_ObjectHolder.MoveObject;
    private ICharaTypeHolder m_Type;
    private ICharaLastActionHolder m_CharaLastActionHolder;

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
    /// 移動目標座標
    /// </summary>
    private Vector3 DestinationPos { get; set; }

    /// <summary>
    /// 移動中かどうか
    /// </summary>
    private bool IsMoving { get; set; }

    private static readonly float SPEED_MAG = 1.5f;
    public static readonly float OFFSET_Y = 0.51f;

    /// <summary>
    /// 攻撃前に呼ばれる
    /// </summary>
    private Subject<Unit> m_OnMoveStart = new Subject<Unit>();
    IObservable<Unit> ICharaMoveEvent.OnMoveStart => m_OnMoveStart;

    /// <summary>
    /// 攻撃後に呼ばれる
    /// </summary>
    private Subject<Unit> m_OnMoveEnd = new Subject<Unit>();
    IObservable<Unit> ICharaMoveEvent.OnMoveEnd => m_OnMoveEnd;

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

        Direction = DIRECTION.UNDER;
        LastMoveDirection = DIRECTION.UNDER;
        var pos = MoveObject.transform.position;
        Position = new Vector3Int((int)pos.x, 0, (int)pos.z);

        PlayerLoopManager.Interface.GetUpdateEvent.Subscribe(_ => Moving()).AddTo(CompositeDisposable);
    }

    /// <summary>
    /// 向きを変える
    /// </summary>
    /// <param name="direction"></param>
    private void Face(DIRECTION direction)
    {
        Direction = direction;
        CharaObject.transform.rotation = Quaternion.LookRotation(direction.ToV3Int());
    }
    void ICharaMove.Face(DIRECTION direction) => Face(direction);

    /// <summary>
    /// 移動
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    bool ICharaMove.Move(DIRECTION direction)
    {
        //向きを変える
        Face(direction);

        //壁抜けはできない
        if (DungeonHandler.Interface.CanMove(Position, direction) == false)
            return false;

        Vector3Int destinationPos = Position + direction.ToV3Int();

        // 他ユニットがいる場合
        if (UnitFinder.Interface.TryGetSpecifiedPositionUnit(destinationPos, out var unit) == true)
        {
            var type = unit.GetInterface<ICharaTypeHolder>().Type;
            if (type != m_Type.Type)
                return false;
            else
            {
                var last = unit.GetInterface<ICharaLastActionHolder>();
                // ターン消費していないなら入れ違い
                if (last.LastAction != CHARA_ACTION.NONE)
                    return false; // 入れ違いできないなら移動不可

                var move = unit.GetInterface<ICharaMove>();
                move.ForcedMove(direction.ToOppsiteDir());
            }
        }

        MoveInternal(destinationPos, direction);
        return true;
    }

    private void MoveInternal(Vector3Int dest, DIRECTION dir)
    {
        // 座標設定
        DestinationPos = dest + new Vector3(0f, OFFSET_Y, 0f);
        LastMoveDirection = dir;

        // 内部的には先に移動しとく
        Position = dest;

        // 移動開始イベント
        m_OnMoveStart.OnNext(Unit.Default);

        // フラグオン
        IsMoving = true;

        // アクション登録
        m_CharaLastActionHolder.RegisterAction(CHARA_ACTION.MOVE);
    }

    void ICharaMove.ForcedMove(DIRECTION dir)
    {
        Face(dir);
        Vector3Int destinationPos = Position + dir.ToV3Int();
        MoveInternal(destinationPos, dir);
    }

    /// <summary>
    /// 移動中処理
    /// </summary>
    private void Moving()
    {
        if (IsMoving == false)
            return;

        MoveObject.transform.position = Vector3.MoveTowards(MoveObject.transform.position, DestinationPos, Time.deltaTime * SPEED_MAG);

        if ((MoveObject.transform.position - DestinationPos).magnitude <= 0.01f)
            FinishMove();
    }

    /// <summary>
    /// 移動終わり
    /// </summary>
    private void FinishMove()
    {
        IsMoving = false;
        m_OnMoveEnd.OnNext(Unit.Default);
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
}