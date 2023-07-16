using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(menuName = "MyScriptable/Dungeon/CurrentCharacter")]
public class CurrentCharacterHolder : ScriptableObject
{
    /// <summary>
    /// リーダーセットアップ
    /// </summary>
    [SerializeField]
    [Expandable]
    private CharacterSetup m_Leader;
    public CharacterSetup Leader { get => m_Leader; set => m_Leader = value; }

    /// <summary>
    /// フレンドセットアップ
    /// </summary>
    [SerializeField]
    [Expandable]
    private CharacterSetup[] m_Friend;
    public CharacterSetup GetFriend(int index) => m_Friend[index];
}
