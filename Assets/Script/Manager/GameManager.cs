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
    DUNGEON_THEME DungeonTheme { get; set; }
    CHARA_NAME LeaderName { get; set; }

    IObservable<Unit> GetInitEvent { get; }
    IObservable<Unit> GetUpdateEvent { get; }
    IObservable<int> FloorChanged { get; }

    Task NextFloor();
}

public class GameManager : Singleton<GameManager, IGameManager>, IGameManager
{
    //現在のダンジョンテーマ
    [SerializeField]
    private DUNGEON_THEME m_DungeonTheme;
    DUNGEON_THEME IGameManager.DungeonTheme
    {
        get { return m_DungeonTheme; }
        set { m_DungeonTheme = value; }
    }

    CHARA_NAME IGameManager.LeaderName { get; set; } = CHARA_NAME.BOXMAN;

    /// <summary>
    /// 現在のフロア階層
    /// </summary>
    private ReactiveProperty<int> m_FloorNum = new ReactiveProperty<int>(1);
    IObservable<int> IGameManager.FloorChanged => m_FloorNum;

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

    /// <summary>
    /// 次の階
    /// </summary>
    /// <returns></returns>
    async Task IGameManager.NextFloor()
    {
        YesorNoQuestionUiManager.Interface.Deactive();

        // 階層up
        m_FloorNum.Value++;

        // 暗転 & ダンジョン再構築
        await FadeManager.Interface.NextFloor(() => RebuildDungeon(), m_FloorNum.Value, m_DungeonTheme.ToString());

        // 行動許可
        TurnManager.Interface.AllCharaActionable();
    }

    /// <summary>
    /// ダンジョン再構築
    /// </summary>
    private void RebuildDungeon()
    {
        // ダンジョン撤去
        DungeonManager.Instance.RemoveDungeon();
        DungeonContentsDeployer.Interface.Remove();

        // ダンジョン再構築
        DungeonManager.Instance.DeployDungeon();
        DungeonContentsDeployer.Interface.Deploy();
    }
}
