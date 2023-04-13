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
    public CharacterSetup Leader => m_Leader;

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

    protected override void Awake()
    {
        m_Initialize.Subscribe(_ =>
        {
            SoundManager.Instance.BlueCrossBGM.Play();
        }).AddTo(this);
    }

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
