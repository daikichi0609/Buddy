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

    MUSHROOM, // きのこ
    RADISH, // 大根
    CAT_WITCH, // 猫魔女
    CRYSTAL_GOLEM // クリスタルゴーレム
}

public static class SceneName
{
    public static readonly string SCENE_HOME = "Home";
    public static readonly string SCENE_DUNGEON = "Dungeon";
    public static readonly string SCENE_CHECKPOINT = "CheckPoint";
    public static readonly string SCENE_BOSS_BATTLE = "Bossbattle";
}

public static class KeyName
{
    public static readonly string BUFF = "Buff";
}