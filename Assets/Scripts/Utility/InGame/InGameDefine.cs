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
    public enum SCENE_NAME
    {
        NONE = -1,
        HOME = 0,
        DUNGEON = 1,
        CHECKPOINT = 2,
        BOSS_BATTLE = 3,
        FINAL_BOSS_BATTLE = 4,
    }

    public static string GetSceneName(this SCENE_NAME name)
    {
        return name switch
        {
            SCENE_NAME.HOME => SCENE_HOME,
            SCENE_NAME.DUNGEON => SCENE_DUNGEON,
            SCENE_NAME.CHECKPOINT => SCENE_CHECKPOINT,
            SCENE_NAME.BOSS_BATTLE => SCENE_BOSS_BATTLE,
            SCENE_NAME.FINAL_BOSS_BATTLE => SCENE_FINAL_BOSS_BATTLE,
            _ => string.Empty,
        };
    }

    public static readonly string SCENE_HOME = "Home";
    public static readonly string SCENE_DUNGEON = "Dungeon";
    public static readonly string SCENE_CHECKPOINT = "CheckPoint";
    public static readonly string SCENE_BOSS_BATTLE = "BossBattle";
    public static readonly string SCENE_FINAL_BOSS_BATTLE = "FinalBossBattle";
}

public static class KeyName
{
    public static readonly string BUFF = "Buff";
}