using UnityEngine;
using UniRx;
using System;

public interface ICharaMove : ICharacterComponent
{
    Vector3 Position { get; }
    Vector3 Direction { get; }
    Vector3 LastMoveDirection { get; }

    void Face(Vector3 direction);
    bool Move(Vector3 direction);
    void Wait();

    void Warp(Vector3 pos);
}

public class CharaMove : CharaComponentBase, ICharaMove
{
    private ICharaObjectHolder m_Holder;
    private GameObject CharaObject => m_Holder.CharaObject;
    private GameObject MoveObject => m_Holder.MoveObject;

    private ICharaTurn m_CharaTurn;

    private ICharaAnimator m_CharaAnimator;

	/// <summary>
	/// 位置
	/// </summary>
	private Vector3 Position { get; set; }
    Vector3 ICharaMove.Position => Position;

	/// <summary>
    /// 向いている方向
    /// </summary>
	private Vector3 Direction { get; set; }
    Vector3 ICharaMove.Direction => Direction;

    /// <summary>
    /// 前回の移動した方向
    /// </summary>
    private Vector3 LastMoveDirection { get; set; }
    Vector3 ICharaMove.LastMoveDirection => LastMoveDirection;

    /// <summary>
    /// 移動目標座標
    /// </summary>
    private Vector3 DestinationPos { get; set; }

    /// <summary>
    /// 移動中かどうか
    /// </summary>
    private bool IsMoving { get; set; }

    /// <summary>
    /// 移動後イベントタイプ
    /// </summary>
    private MOVE_FINISH_EVENT_TYPE m_EventType;
    public enum MOVE_FINISH_EVENT_TYPE
    {
        NONE,
        ITEM,
        TRAP,
    }

    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <param name="inventoryCount"></param>
    protected override void Initialize()
    {
        m_Holder = Collector.GetComponent<ICharaObjectHolder>();
        m_CharaTurn = Collector.GetComponent<ICharaTurn>();
        m_CharaAnimator = Collector.GetComponent<ICharaAnimator>();

        Direction = new Vector3(0, 0, -1);
        Position = MoveObject.transform.position;

        GameManager.Instance.GetUpdate.Subscribe(_ => Moving());
    }

    /// <summary>
    /// 向きを変える
    /// </summary>
    /// <param name="direction"></param>
    private void Face(Vector3 direction)
    {
        Direction = direction;
        CharaObject.transform.rotation = Quaternion.LookRotation(direction);
    }
    void ICharaMove.Face(Vector3 direction) => Face(direction);

    /// <summary>
    /// 移動
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    bool ICharaMove.Move(Vector3 direction)
    {
        //行動中ならできない
        if (m_CharaTurn.IsActing == true)
            return false;

        //向きを変える
        Face(direction);

        //壁抜けはできない
        if (DungeonHandler.Interface.CanMoveDiagonal(Position, direction) == false)
            return false;

        Vector3 destinationPos = Position + direction;

        // 他ユニットがいるなら移動不可
        if (UnitManager.Interface.TryGetSpecifiedPositionUnit(destinationPos, out var unit) == true)
            return false;

        // 移動成功
        // Actionフラグオン
        m_CharaTurn.StartAction();

        //目標座標設定
        DestinationPos = destinationPos;

        LastMoveDirection = direction;

        //内部的には先に移動しとく
        Position = DestinationPos;

        //移動開始
        m_CharaAnimator.PlayAnimation(ANIMATION_TYPE.MOVE);
        IsMoving = true;

        //移動終わる前に現在マスチェックを済ませる
        //移動後何かする必要あるなら、それを覚える
        if(CheckCurrentGrid(out m_EventType) == true)
            m_CharaTurn.StartAction();

        //ターンを返す
        m_CharaTurn.FinishTurn();

        return true;
    }

    /// <summary>
    /// 移動中処理
    /// </summary>
    private void Moving()
    {
        if (IsMoving == false)
            return;

        MoveObject.transform.position = Vector3.MoveTowards(MoveObject.transform.position, DestinationPos, Time.deltaTime * 3);

        if ((MoveObject.transform.position - DestinationPos).magnitude <= 0.01f)
            FinishMove();
    }

    /// <summary>
    /// 移動終わり
    /// </summary>
    private void FinishMove()
    {
        IsMoving = false;
        m_CharaTurn.FinishAction();
        m_CharaAnimator.StopAnimation(ANIMATION_TYPE.MOVE);
        MoveObject.transform.position = Position;
    }

    /// <summary>
    /// 待機 ターン終了するだけ
    /// </summary>
    void ICharaMove.Wait() => m_CharaTurn.FinishTurn();

    /// <summary>
    /// ワープ
    /// </summary>
    /// <param name="pos"></param>
    void ICharaMove.Warp(Vector3 pos)
    {
        Position = pos;
        m_Holder.MoveObject.transform.position = pos;
    }

    /// <summary>
    /// 現在地マスのイベント処理
    /// </summary>
    private bool CheckCurrentGrid(out MOVE_FINISH_EVENT_TYPE type)
    {
        type = MOVE_FINISH_EVENT_TYPE.NONE;

        /* ここでやらない
        //メインプレイヤーなら
        if (this.gameObject == ObjectManager.Instance.m_PlayerList[0])
        {
            //階段チェック
            if (DungeonTerrain.Instance.GridID((int)Position.x, (int)Position.z) == (int)DungeonTerrain.GRID_ID.STAIRS)
            {
                QuestionManager.Instance.Log = new StairsLog();
                QuestionManager.Instance.GetManager.IsActive = true;
                return true;
            }
        }
        */

        /*
        //アイテムチェック
        foreach (GameObject obj in UnitManager.Instance.ItemList)
        {
            Vector3 pos = obj.GetComponent<Item>().Position;

            if (pos.x == Position.x && pos.z == Position.z)
            {
                BagManager.Instance.GetManager.PutAway(obj);
                return true;
            }
        }
        */

        //罠チェック


        return false;
    }
}