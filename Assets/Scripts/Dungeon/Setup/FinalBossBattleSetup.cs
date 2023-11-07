using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(menuName = "MyScriptable/Dungeon/FinalBossBattle")]
public class FinalBossBattleSetup : ScriptableObject
{
    /// <summary>
    /// キング戦
    /// </summary>
    [SerializeField, Header("キング戦")]
    private BossBattleSetup m_KingBossBattleSetup;
    public BossBattleSetup KingBossBattleSetup => m_KingBossBattleSetup;

    /// <summary>
    /// ウォーリア
    /// </summary>
    [SerializeField, Header("ウォーリア")]
    private CharacterSetup m_WarriorSetup;
    public CharacterSetup WarriorSetup => m_WarriorSetup;

    /// <summary>
    /// キング戦助っ人
    /// </summary>
    [SerializeField, Header("キング戦助っ人")]
    private CharacterSetup[] m_FriendSetup;
    public CharacterSetup[] FriendSetup => m_FriendSetup;

    /// <summary>
    /// バルム戦
    /// </summary>
    [SerializeField, Header("バルム戦")]
    private BossBattleSetup m_BarmBossBattleSetup;
    public BossBattleSetup BarmBossBattleSetup => m_BarmBossBattleSetup;

    /// <summary>
    /// タイムラインステージ
    /// </summary>
    [SerializeField, Header("ステージ")]
    private GameObject m_Stage;
    public GameObject Stage => m_Stage;

    /// <summary>
    /// フレンドインデックス
    /// </summary>
    [SerializeField, Header("フレンドインデックス")]
    private int m_FriendIndex;
    public int FriendIndex { get => m_FriendIndex; set => m_FriendIndex = value; }

    /// <summary>
    /// 負けフラグ
    /// </summary>
    [SerializeField, Header("負けフラグ")]
    private bool m_IsLoseBack;
    public bool IsLoseBack { get => m_IsLoseBack; set => m_IsLoseBack = value; }

    /// <summary>
    /// 負けフラグ通過済み
    /// </summary>
    [SerializeField, Header("負けフラグ")]
    private bool m_IsLoseBackComplete;
    public bool IsLoseBackComplete { get => m_IsLoseBackComplete; set => m_IsLoseBackComplete = value; }
}
