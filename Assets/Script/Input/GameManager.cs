using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UniRx;
using UniRx.Triggers;
using System;
using System.Threading.Tasks;

public interface IGameManager : ISingleton
{
    CharacterSetup Leader { get; }
    CharacterSetup Friend { get; }

    IObservable<Unit> GetInitEvent { get; }
    IObservable<Unit> GetUpdateEvent { get; }
}

public class GameManager : Singleton<GameManager, IGameManager>, IGameManager
{
    /// <summary>
    /// リーダーセットアップ
    /// </summary>
    [SerializeField]
    private CharacterSetup m_Leader;
    CharacterSetup IGameManager.Leader => m_Leader;

    /// <summary>
    /// フレンドセットアップ
    /// </summary>
    [SerializeField]
    private CharacterSetup m_Friend;
    CharacterSetup IGameManager.Friend => m_Friend;

    /// <summary>
    /// 初期化処理メソッドまとめ
    /// </summary>
    private Subject<Unit> m_Initialize = new Subject<Unit>();
    IObservable<Unit> IGameManager.GetInitEvent => m_Initialize;

    /// <summary>
    /// 毎F処理メソッドまとめ
    /// </summary>
    private Subject<Unit> m_Update = new Subject<Unit>();
    IObservable<Unit> IGameManager.GetUpdateEvent => m_Update;

    //初期化処理呼び出し
    private void Start()
    {
        m_Initialize.OnNext(Unit.Default);
    }

    //Update関数は基本的にここだけ
    private void Update()
    {
        m_Update.OnNext(Unit.Default);
    }
}
