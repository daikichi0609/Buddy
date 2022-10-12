public static class Define
{
	// キャラ名
	public enum CHARA_NAME
	{
		BOXMAN,
		RAGON,
		ICHIGO,
		WIZARD,
		ARCHER,

		MASHROOM
	}

	// アイテム名
	public enum ITEM_NAME
    {
		APPLE,
    }

	//ダンジョンテーマ
	public enum DUNGEON_THEME
	{
		GRASS,
		ROCK,
		CRYSTAL,
		WHITE
	}

	//ダンジョン名
	public enum DUNGEON_NAME
	{
		始まりの森,
		岩場,
		クリスタル,
		白
	}
}

public static class InternalDefine
{
	//ゲーム全体のステート
	public enum GAME_STATE
	{
		LOADING,
		PLAYING
	}

	//実行する行動タイプ
	public enum ACTION
	{
		ATTACK,
		SKILL,
		MOVE,
	}

	/// <summary>
    /// 目標にするターゲット
    /// </summary>
	public enum TARGET
	{
		PLAYER,
		ENEMY,
		NONE
	}

	/// <summary>
    /// 敵の行動タイプ
    /// </summary>
	public enum ENEMY_STATE
	{
		NONE,
		SEARCHING,
		CHASING,
		ATTACKING
	}
}

public static class Message
{
	/// <summary>
    /// 暗転・明転のリクエスト用
    /// </summary>
	public struct MRequestBlackPanel
	{
		public bool IsDark
		{
			get;
			set;
		}
	}

	/// <summary>
    /// 明転・暗転終了通知
    /// </summary>
	public struct MFinishBlackPanel
    {
		public bool IsDark
        {
			get;
			set;
        }
    }

	/// <summary>
    /// FloorText表示処理終了通知
    /// 実質ダンジョン再構築完了通知
    /// </summary>
	public struct MFinishFloorText
    {

    }

	/// <summary>
    /// ターン終了通知
    /// </summary>
	public struct MFinishTurn
    {
		
    }

	/// <summary>
    /// ダメージ終了
    /// </summary>
	public struct MFinishDamage
    {
		public MFinishDamage(CharaBattle chara, bool isHit, bool isDead)
        {
			Chara = chara;
			IsHit = isHit;
			IsDead = isDead;
        }

		/// <summary>
        /// 攻撃元のキャラ
        /// </summary>
		public CharaBattle Chara
        {
			get;
        }

		/// <summary>
        /// 攻撃がヒットしたかどうか
        /// </summary>
        public bool IsHit
        {
			get;
        }

		/// <summary>
        /// 死亡したかどうか
        /// </summary>
		public bool IsDead
        {
			get;
        }
    }
}