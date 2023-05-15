using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IOutGameInfoHolder : ISingleton
{
    CharacterSetup Leader { get; }
    CharacterSetup Friend { get; }
}

public class OutGameInfoHolder : Singleton<OutGameInfoHolder, IOutGameInfoHolder>, IOutGameInfoHolder
{
    /// <summary>
    /// リーダーセットアップ
    /// </summary>
    [SerializeField]
    private CharacterSetup m_Leader;
    CharacterSetup IOutGameInfoHolder.Leader => m_Leader;

    /// <summary>
    /// フレンドセットアップ
    /// </summary>
    [SerializeField]
    private CharacterSetup m_Friend;
    CharacterSetup IOutGameInfoHolder.Friend => m_Friend;
}
