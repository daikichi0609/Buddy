using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class InputManager : Singleton<InputManager>
{
	//移動方向
	private Vector3 Direction { get; set; }

	//追加入力受付用タイマー
	private float Timer { get; set; }

	//追加入力受付用フラグ
	private bool IsWaitingAdditionalInput { get; set; }

	/// <summary>
	/// UI表示中かどうか
	/// </summary>
	public bool IsUiPopUp => QuestionManager.Instance.GetManager.IsActive || MenuManager.Instance.GetManager.IsActive || BagManager.Instance.GetManager.IsActive;

	/// <summary>
	/// 重複入力を禁止するためのフラグ
	/// </summary>
	public bool IsProhibitDuplicateInput { get; set; }

	protected override void Awake()
    {
        base.Awake();

		GameManager.Instance.GetUpdate
			.Subscribe(_ => DetectInput()).AddTo(this);
    }

    //入力受付メソッド
    private void DetectInput()
    {
		//入力禁止中なら入力を受け付けない
		if (IsProhibitDuplicateInput == true)
		{
			IsProhibitDuplicateInput = false;
			return;
		}

		//プレイヤーキャラ取得
		var player = UnitManager.Interface.PlayerList[0];
		var move = player.GetComponent<ICharaMove>();
		var turn = player.GetComponent<ICharaTurn>();
		var battle = player.GetComponent<ICharaBattle>();

		//操作対象キャラのターンが終わっている場合、行動が禁じられている場合、UI表示中の場合は入力を受け付けない
		if (turn.IsFinishTurn == true || TurnManager.Interface.CanAct == false || IsUiPopUp == true)
			return;

		//メニューを開く
		if (Input.GetKeyDown(KeyCode.Q))
		{
			IsProhibitDuplicateInput = true;
			MenuManager.Instance.GetManager.IsActive = true;
			return;
		}

		if (IsWaitingAdditionalInput == true)
        {
			DetectAdditionalInput(move);
			return;
        }

		if (Input.GetKey(KeyCode.W))
			Direction = new Vector3(0f, 0f, 1);

		if (Input.GetKey(KeyCode.A))
			Direction = new Vector3(-1f, 0f, 0f);

		if (Input.GetKey(KeyCode.S))
			Direction = new Vector3(0f, 0f, -1);

		if (Input.GetKey(KeyCode.D))
			Direction = new Vector3(1f, 0f, 0f);

		if(Direction != new Vector3(0f, 0f, 0f))
        {
			IsWaitingAdditionalInput = true;
			return;
        }

		var anim = player.GetComponent<ICharaAnimator>();
		if (anim.IsCurrentState("Idle") == false)
			return;

		if (Input.GetKeyDown(KeyCode.E)　&& TurnManager.Interface.CanAct == true)
			battle.NormalAttack();
		
	}

	private void DetectAdditionalInput(ICharaMove move)
    {
		if (Input.GetKey(KeyCode.W) && Direction.z == 0)
			Direction += new Vector3(0f, 0f, 1f);

		if (Input.GetKey(KeyCode.A) && Direction.x == 0)
			Direction += new Vector3(-1f, 0f, 0f);

		if (Input.GetKey(KeyCode.S) && Direction.z == 0)
			Direction += new Vector3(0f, 0f, -1);

		if (Input.GetKey(KeyCode.D) && Direction.x == 0)
			Direction += new Vector3(1f, 0f, 0f);

		Timer += Time.deltaTime;
		if(Timer >= 0.01f || JudgeDirectionDiagonal(Direction) == true)
        {
			move.Move(Direction);
			Direction = new Vector3(0f, 0f, 0f);
			Timer = 0f;
			IsWaitingAdditionalInput = false;
		}
    }

	private bool JudgeDirectionDiagonal(Vector3 direction)
    {
		if(direction == new Vector3(1f, 0f, 1f))
			return true;

		if (direction == new Vector3(1f, 0f, -1f))
			return true;

		if (direction == new Vector3(-1f, 0f, 1f))
			return true;

		if (direction == new Vector3(-1f, 0f, -1f))
			return true;

		return false;
	}
}
