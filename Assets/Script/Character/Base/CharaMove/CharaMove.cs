using UnityEngine;
using UniRx;

public abstract class Chara : MonoBehaviour
{
	[SerializeField] protected bool DEBUG;

	/// <summary>
    /// 実際に動かすオブジェクト
    /// </summary>
	[SerializeField] private GameObject m_MoveObject;
	protected GameObject MoveObject
	{
		get { return m_MoveObject; }
	}

	/// <summary>
    /// キャラのオブジェクト
    /// </summary>
	[SerializeField] private GameObject m_CharaObject;
	protected GameObject CharaObject
	{
		get { return m_CharaObject; }
	}

	protected CharaTurn CharaTurn
    {
		get;
		set;
    }

	/// <summary>
	/// 位置
	/// </summary>
    [SerializeField] private Vector3 m_Position;
	public Vector3 Position
	{
		get => m_Position;
		set => m_Position = value;
	}

	/// <summary>
    /// 向いている方向
    /// </summary>
	public Vector3 Direction
	{
		get;
		set;
	}

	/// <summary>
    /// キャラが持つアニメーター
    /// </summary>
	protected Animator m_CharaAnimator;
	public Animator CharaAnimator
	{
		get { return m_CharaAnimator; }
		protected set { m_CharaAnimator = value; }
	}

	/// <summary>
    /// 初期化処理
    /// </summary>
    /// <param name="inventoryCount"></param>
	public virtual void Initialize(int inventoryCount)
	{
		CharaAnimator = CharaObject.GetComponent<Animator>();
		Direction = new Vector3(0, 0, -1);
		Position = MoveObject.transform.position;
		CharaTurn = GetComponent<CharaTurn>();
	}

	/// <summary>
    /// 行動を確定する
    /// </summary>
	public void DecideAction()
	{
		CharaTurn.StartAction();
	}
}

public class CharaMove: Chara
{
	/// <summary>
    /// 移動目標座標
    /// </summary>
	public Vector3 DestinationPos
    {
		get;
		private set;
    }

	/// <summary>
    /// 移動中かどうか
    /// </summary>
	private bool IsMoving
    {
		get;
		set;
    }

    public override void Initialize(int inventoryCount)
    {
        base.Initialize(inventoryCount);

		GameManager.Instance.GetUpdate.Subscribe(_ => Moving());
    }

	/// <summary>
    /// 向きを変える
    /// </summary>
    /// <param name="direction"></param>
	public void Face(Vector3 direction)
	{
		Direction = direction;
		CharaObject.transform.rotation = Quaternion.LookRotation(direction);
	}

	/// <summary>
    /// 移動
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
	public bool Move(Vector3 direction)
	{
		//行動中ならできない
		if(CharaTurn.IsActing == true)
        {
			return false;
        }

		//向きを変える
		Face(direction);

		//壁抜けはできない
		if (Positional.IsPossibleToMoveGrid(Position, direction) == false)
		{
			return false;
		}

		//敵をすり抜けはできない
		Vector3 destinationPos = Position + direction;
		if (Positional.IsEnemyOn(destinationPos) == true)
		{
			return false;
		}

		CharaTurn.StartAction();

		//移動開始
		CharaAnimator.SetBool("IsRunning", true);
		IsMoving = true;
		CharaTurn.CAN_ATTACK = false;

		//目標座標設定
		DestinationPos = destinationPos;

		//内部的には先に移動しとく
		Position = DestinationPos;

		//移動終わる前に現在マスチェックを済ませる
		CheckCurrentGrid();

		//行動確定
		DecideAction();
		//ターンを返す
		CharaTurn.FinishTurn();

		return true;
	}

	/// <summary>
    /// 移動中処理
    /// </summary>
	private void Moving()
	{
		if(IsMoving == false)
        {
			return;
        }

		MoveObject.transform.position = Vector3.MoveTowards(MoveObject.transform.position, DestinationPos, Time.deltaTime * 3);

		if ((MoveObject.transform.position - DestinationPos).magnitude <= 0.01f)
		{
			FinishMove();
		}
	}

	/// <summary>
    /// 移動終わり
    /// </summary>
	private void FinishMove()
	{
		CharaTurn.CAN_ATTACK = true;
		CharaTurn.FinishAction();
		IsMoving = false;
		CharaAnimator.SetBool("IsRunning", false);
		MoveObject.transform.position = Position;
	}

	/// <summary>
    /// 待機 ターン終了するだけ
    /// </summary>
	public void Wait()
    {
		CharaTurn.FinishTurn();
    }

	/// <summary>
    /// 現在地マスのイベント処理
    /// </summary>
	public void CheckCurrentGrid()
    {
		//メインプレイヤーなら
		if (this.gameObject == ObjectManager.Instance.m_PlayerList[0])
		{
			//階段チェック
			if (DungeonTerrain.Instance.GridID((int)Position.x, (int)Position.z) == (int)DungeonTerrain.GRID_ID.STAIRS)
			{
				QuestionManager.Instance.Log = new StairsLog();
				QuestionManager.Instance.GetManager.IsActive = true;
				return;
			}

			//アイテムチェック
			foreach(GameObject obj in ObjectManager.Instance.ItemList)
            {
				Vector3 pos = obj.GetComponent<Item>().Position;

				if(pos.x == Position.x && pos.z == Position.z)
                {
					BagManager.Instance.GetManager.PutAway(obj);
					return;
                }
            }

			//罠チェック

		}
    }
}