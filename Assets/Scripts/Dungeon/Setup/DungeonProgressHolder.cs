using UnityEngine;
using NaughtyAttributes;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "MyScriptable/Dungeon/Progress")]
public class DungeonProgressHolder : ScriptableObject
{
    /// <summary>
    /// ダンジョンセットアップ集
    /// </summary>
    [SerializeField]
    private DungeonSetupHolder[] m_DungeonSetupHolders = new DungeonSetupHolder[0];
    private DungeonSetupHolder CurrentDungeonSetupHolder => m_DungeonSetupHolders[(int)m_CurrentDungeonTheme];
    public DungeonSetup CurrentDungeonSetup => CurrentDungeonSetupHolder.DungeonSetup[m_CurrentDungeonProgress];
    public CheckPointSetup CurrentCheckPointSetup => CurrentDungeonSetupHolder.DungeonSetup[m_CurrentDungeonProgress - 1].CheckPointSetup;
    public BossBattleSetup CurrentBossBattleSetup => CurrentDungeonSetupHolder.BossBattleSetup;

    /// <summary>
    /// 最終ボスセットアップ
    /// </summary>
    [SerializeField]
    private FinalBossBattleSetup m_FinalBossBattleSetup;
    public FinalBossBattleSetup FinalBossBattleSetup => m_FinalBossBattleSetup;

    /// <summary>
    /// 現在のダンジョンテーマ
    /// </summary>
    [SerializeField, ReadOnly]
    private DUNGEON_THEME m_CurrentDungeonTheme;
    public void SetCurrentDungeonTheme(DUNGEON_THEME theme) => m_CurrentDungeonTheme = theme;

    /// <summary>
    /// 現在のダンジョン進行度
    /// </summary>
    // [ShowNonSerializedField]
    [SerializeField]
    private int m_CurrentDungeonProgress;
    public int CurrentDungeonProgress { get => m_CurrentDungeonProgress; set => m_CurrentDungeonProgress = value; }
    private int MaxDungeonProgress => CurrentDungeonSetupHolder.DungeonSetup.Length;
    public bool IsMaxDungeonProgress => CurrentDungeonProgress == MaxDungeonProgress;

    /// <summary>
    /// ランダムな敵キャラセットアップを重み抽選
    /// </summary>
    /// <returns></returns>
    public CharacterSetup GetRandomEnemySetup()
    {
        var t = CurrentDungeonSetup.EnemyTable;
        return t.GetRandomEnemySetup();
    }

    /// <summary>
    /// アイテムセットアップを重み抽選
    /// </summary>
    /// <returns></returns>
    public ItemSetup GetRandomItemSetup()
    {
        var d = CurrentDungeonSetupHolder.ItemDeploySetup;
        int length = d.ItemPacks.Length;
        int[] weights = new int[length];

        for (int i = 0; i < length; i++)
            weights[i] = d.ItemPacks[i].Weight;

        var index = WeightedRandomSelector.SelectIndex(weights);
        return d.ItemPacks[index].Setup;
    }

    /// <summary>
    /// 罠セットアップを重み抽選
    /// </summary>
    /// <returns></returns>
    public TrapSetup GetRandomTrapSetup()
    {
        var d = CurrentDungeonSetupHolder.TrapDeploySetup;
        int length = d.TrapPacks.Length;
        int[] weights = new int[length];

        for (int i = 0; i < length; i++)
            weights[i] = d.TrapPacks[i].Weight;

        var index = WeightedRandomSelector.SelectIndex(weights);
        return d.TrapPacks[index].Setup;
    }

    /// <summary>
    /// リセット
    /// </summary>
    [Button]
    public void ResetAll()
    {
        m_CurrentDungeonTheme = DUNGEON_THEME.NONE;
        m_CurrentDungeonProgress = 0;

#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
#endif
    }
}
