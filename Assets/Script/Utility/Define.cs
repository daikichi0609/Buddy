public static class Define
{
	// キャラ名
	public enum CHARA_NAME
	{
		BOXMAN, // 段ボール
		RAGON, // リザードマン
		BERRY, // いちご
		DORCHE, // 魔術師

		BALE, // 熊
		LAMY, // 吸血鬼
		PLISS, // 聖職者

		KING, // 王様
		BARM, // 騎士

		MASHROOM, // きのこ
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
	public enum CHARA_TYPE
	{
		PLAYER,
		ENEMY,
		NONE
	}
}