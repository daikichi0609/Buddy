using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UniRx;
using UniRx.Triggers;
using System;

public class GameManager : SingletonMonoBehaviour<GameManager>
{
    //現在のゲームステート
    [SerializeField] private InternalDefine.GAME_STATE m_CurrentGameState;
    public InternalDefine.GAME_STATE CurrentGameState
    {
        get { return m_CurrentGameState; }
        set { m_CurrentGameState = value; }
    }

    //現在のダンジョンテーマ
    [SerializeField] private Define.DUNGEON_THEME m_DungeonTheme;
    public Define.DUNGEON_THEME DungeonTheme
    {
        get { return m_DungeonTheme; }
        set { m_DungeonTheme = value; }
    }

    //現在のダンジョン名
    public Define.DUNGEON_NAME DungeonName
    {
        get; set;
    }

    //現在のフロア階数
    [SerializeField] private int m_FloorNum = 1;
    public int FloorNum
    {
        get { return m_FloorNum; }
        set { m_FloorNum = value; }
    }

    //リーダーキャラの名前
    [SerializeField] private Define.CHARA_NAME m_LeaderName;
    public Define.CHARA_NAME LeaderName
    {
        get { return m_LeaderName; }
    }

    /// <summary>
    /// 初期化処理メソッドまとめ
    /// </summary>
    private Subject<Unit> m_Initialize = new Subject<Unit>();

    public IObservable<Unit> GetInit
    {
        get { return m_Initialize; }
    }

    /// <summary>
    /// ダンジョン再構築メソッドまとめ
    /// </summary>
    private Subject<Unit> m_ReInitialize = new Subject<Unit>();

    public IObservable<Unit> GetReInit
    {
        get { return m_ReInitialize; }
    }

    /// <summary>
    /// 毎F処理メソッドまとめ
    /// </summary>
    private Subject<Unit> m_Update = new Subject<Unit>();

    public IObservable<Unit> GetUpdate
    {
        get { return m_Update; }
    }

    protected override void Awake()
    {
        GetInit.Subscribe(_ =>
        {
            //暫定処理たくさん
            m_LeaderName = Define.CHARA_NAME.BOXMAN;

            SoundManager.Instance.BlueCrossBGM.Play();
        }).AddTo(this);

        //暗転終了通知
        MessageBroker.Default.Receive<Message.MFinishBlackPanel>()
            .Where(_ => _.IsDark == true)
            .Subscribe(_ => ReInitialize()).AddTo(this);

        //明転終了通知
        MessageBroker.Default.Receive<Message.MFinishBlackPanel>()
            .Where(_ => _.IsDark == false)
            .Subscribe(_ => ReStartGame()).AddTo(this);

        //ダンジョン再構築終了通知
        MessageBroker.Default.Receive<Message.MFinishFloorText>()
            .Subscribe(_ =>  MessageBroker.Default
                .Publish(new Message.MRequestBlackPanel { IsDark = false }));
    }

    //初期化処理呼び出し
    private void Start()
    {
        m_Initialize.OnNext(Unit.Default);
    }

    //次の階に行くときの初期化
    private void ReInitialize()
    {
        Debug.Log("再初期化");
        RemoveDungeon();
        RebuildDungeon();
        TurnManager.Instance.AllCharaActionable();
        BlackPanelManager.Instance.ControllText();
    }

    //Update関数は基本的にここだけ
    private void Update()
    {
        m_Update.OnNext(Unit.Default);
    }

    //ダンジョン再構築
    private void RebuildDungeon()
    {
        Debug.Log("Rebuild");
        DungeonTerrain.Instance.DeployDungeon();
        DungeonContents.Instance.RedeployDungeonContents();
    }

    //ダンジョン破壊
    private void RemoveDungeon()
    {
        DungeonTerrain.Instance.RemoveDungeon();
        DungeonContents.Instance.RemoveDungeonContents();
    }

    //次の階にいくときの処理
    public void UpToNextFloor()
    {
        FloorNum++;
        CurrentGameState = InternalDefine.GAME_STATE.LOADING;
        MessageBroker.Default.Publish(new Message.MRequestBlackPanel { IsDark = true　});
    }

    //明転
    private void ReStartGame()
    {
         CurrentGameState = InternalDefine.GAME_STATE.PLAYING;
    }
}
