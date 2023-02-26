/// <summary>
/// キャラ名
/// </summary>
public enum CHARA_NAME
{
    NONE,
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

/// <summary>
/// アイテム名
/// </summary>
public enum ITEM_NAME
{
    APPLE,
}

/// <summary>
/// ダンジョン種類
/// </summary>
public enum DUNGEON_THEME
{
    GRASS = 0,
    ROCK = 1,
    CRYSTAL = 2,
    WHITE = 3,
}

/// <summary>
/// キャラタイプ
/// </summary>
public enum CHARA_TYPE
{
    PLAYER,
    ENEMY,
    NONE
}