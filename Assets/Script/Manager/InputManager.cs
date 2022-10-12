using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class InputManager : SingletonMonoBehaviour<InputManager>
{
	//移動方向
	private Vector3 Direction
    {
		get;
		set;
    }

	//追加入力受付用タイマー
	private float Timer
    {
		get;
		set;
    }

	//追加入力受付用フラグ
	private bool IsWaitingAdditionalInput
    {
		get;
		set;
    }

	/// <summary>
	/// UI表示中かどうか
	/// </summary>
	public bool IsUiPopUp => QuestionManager.Instance.GetManager.IsActive || MenuManager.Instance.GetManager.IsActive || BagManager.Instance.GetManager.IsActive;

	/// <summary>
	/// 重複入力を禁止するためのフラグ
	/// </summary>
	public bool IsProhibitDuplicateInput
	{
		get;
		set;
	}

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
		GameObject chara = ObjectManager.Instance.PlayerObject(0);
		CharaMove playerMove = chara.GetComponent<CharaMove>();
		CharaTurn charaTurn = chara.GetComponent<CharaTurn>();
		PlayerBattle playerBattle = chara.GetComponent<PlayerBattle>();

		//操作対象キャラのターンが終わっている場合、行動が禁じられている場合、UI表示中の場合は入力を受け付けない
		if (charaTurn.IsFinishTurn == true || TurnManager.Instance.IsCanAction == false || IsUiPopUp == true)
        {
			return;
        }

		//メニューを開く
		if (Input.GetKeyDown(KeyCode.Q))
		{
			IsProhibitDuplicateInput = true;
			MenuManager.Instance.GetManager.IsActive = true;
			return;
		}

		if (IsWaitingAdditionalInput == true)
        {
			DetectAdditionalInput(chara);
			return;
        }

		if (Input.GetKey(KeyCode.W))
		{
			Direction = new Vector3(0f, 0f, 1);
		}
		if (Input.GetKey(KeyCode.A))
		{
			Direction = new Vector3(-1f, 0f, 0f);
		}
		if (Input.GetKey(KeyCode.S))
		{
			Direction = new Vector3(0f, 0f, -1);
		}
		if (Input.GetKey(KeyCode.D))
		{
			Direction = new Vector3(1f, 0f, 0f);
		}

		if(Direction != new Vector3(0f, 0f, 0f))
        {
			IsWaitingAdditionalInput = true;
			return;
        }

		if (playerMove.CharaAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle") == false)
		{
			return;
		}

		if (Input.GetKeyDown(KeyCode.E)　&& TurnManager.Instance.IsCanAttack == true)
		{
			playerBattle.Act(InternalDefine.ACTION.ATTACK);
		}		
	}

	private void DetectAdditionalInput(GameObject chara)
    {
		if (Input.GetKey(KeyCode.W) && Direction.z == 0)
		{
			Direction += new Vector3(0f, 0f, 1f);
		}
		if (Input.GetKey(KeyCode.A) && Direction.x == 0)
		{
			Direction += new Vector3(-1f, 0f, 0f);
		}
		if (Input.GetKey(KeyCode.S) && Direction.z == 0)
		{
			Direction += new Vector3(0f, 0f, -1);
		}
		if (Input.GetKey(KeyCode.D) && Direction.x == 0)
		{
			Direction += new Vector3(1f, 0f, 0f);
		}

		Timer += Time.deltaTime;
		if(Timer >= 0.01f || JudgeDirectionDiagonal(Direction) == true)
        {
			CharaMove playerMove = chara.GetComponent<CharaMove>();
			playerMove.Move(Direction);
			Direction = new Vector3(0f, 0f, 0f);
			Timer = 0f;
			IsWaitingAdditionalInput = false;
		}
    }

	private bool JudgeDirectionDiagonal(Vector3 direction)
    {
		if(direction == new Vector3(1f, 0f, 1f))
        {
			return true;
        }
		if (direction == new Vector3(1f, 0f, -1f))
		{
			return true;
		}
		if (direction == new Vector3(-1f, 0f, 1f))
		{
			return true;
		}
		if (direction == new Vector3(-1f, 0f, -1f))
		{
			return true;
		}

		return false;
	}
}
