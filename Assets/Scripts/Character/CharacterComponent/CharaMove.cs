using UnityEngine;
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
    private ICharaAnimator m_CharaAnimator;

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
        m_CharaAnimator = Owner.GetInterface<ICharaAnimator>();

        // ----- 初期化 ----- //
        Direction = DIRECTION.UNDER;
        LastMoveDirection = DIRECTION.UNDER;
        var pos = MoveObject.transform.position;
        Position = new Vector3Int((int)pos.x, 0, (int)pos.z);

        // アクション登録
        m_OnMoveStart.SubscribeWithState(this, (_, self) => self.m_CharaLastActionHolder.RegisterAction(CHARA_ACTION.MOVE)).AddTo(Owner.Disposables);
    }

    /// <summary>
    /// 向きを変える
    /// </summary>
    /// <param name="direction"></param>
    private Task Face(DIRECTION direction)
    {
        if (direction == DIRECTION.NONE)
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
        // 前の移動タスクの完了を待つ
        if (m_MovingTask != null)
            while (m_MovingTask.IsCompleted == false)
                await Task.Delay(1);

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
            if (m_Type.Type != CHARA_TYPE.FRIEND || unit.GetInterface<ICharaTypeHolder>().Type != CHARA_TYPE.FRIEND)
                return false;
            else
            {
                var last = unit.GetInterface<ICharaLastActionHolder>();
                // ターン消費していないなら入れ違い
                if (last.LastAction != CHARA_ACTION.NONE)
                    return false; // 入れ違いできないなら移動不可

                var move = unit.GetInterface<ICharaMove>();
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
        m_MovingTask = MoveTask(destPos);
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
    private async Task MoveTask(Vector3 dest)
    {
        while ((MoveObject.transform.position - dest).magnitude > 0.01f)
        {
            MoveObject.transform.position = Vector3.MoveTowards(MoveObject.transform.position, dest, Time.deltaTime * SPEED_MAG);
            await Task.Delay(1);
        }
    }

    /// <summary>
    /// 強制的に移動
    /// 入れ替わり用
    /// </summary>
    /// <param name="dir"></param>
    async Task ICharaMove.ForcedMove(DIRECTION dir)
    {
        await m_CharaTurn.WaitFinishActing();
        await Face(dir);
        Vector3Int destinationPos = Position + dir.ToV3Int();
        await MoveInternal(destinationPos, dir);
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