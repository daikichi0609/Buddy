using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using NaughtyAttributes;
using Zenject;

public interface ICharaUiManager : ISingleton
{
    void InitializeCharacterUi(ICollector[] units);
}

public class CharaUiManager : Singleton<CharaUiManager, ICharaUiManager>, ICharaUiManager
{
    [Inject]
    private IDungeonContentsDeployer m_DungeonContentsDeployer;
    [Inject]
    private IUnitHolder m_UnitHolder;

    private static readonly Vector3 ms_Diff = new Vector3(0f, -210f, 0f);

    //各キャラUI格納用List（キャラUiは他とは別）
    [SerializeField, ReadOnly]
    private List<CharaUi> m_CharacterUiList = new List<CharaUi>();
    public List<CharaUi> CharacterUiList
    {
        get { return m_CharacterUiList; }
        set { m_CharacterUiList = value; }
    }

    [Inject]
    public void Construct(IPlayerLoopManager loopManager)
    {
        // Updateで更新
        loopManager.GetUpdateEvent
            .Subscribe(_ => UpdateCharaUi()).AddTo(this);
    }

    protected override void Awake()
    {
        base.Awake();

        // Ui初期化
        m_DungeonContentsDeployer.OnDeployContents.Subscribe(_ =>
        {
            var units = m_UnitHolder.FriendList.ToArray();
            InitializeCharacterUi(units);
        });
    }

    /// <summary>
    /// CharaUi初期化
    /// </summary>
    /// <param name="units"></param>
    public void InitializeCharacterUi(ICollector[] units)
    {
        int i = 0;

        foreach (var unit in units)
        {
            GameObject obj = Instantiate(UiHolder.Instance.CharacterUi);
            obj.transform.SetParent(UiHolder.Instance.Canvas.transform, false);
            obj.transform.SetAsFirstSibling();
            CharaUi ui = obj.GetComponent<CharaUi>();
            CharacterUiList.Add(ui);
            ui.Initialize(unit);

            obj.GetComponent<RectTransform>().transform.position += ms_Diff * i++;
        }
    }

    /// <summary>
    /// Ui更新
    /// </summary>
    private void UpdateCharaUi()
    {
        foreach (var chara in CharacterUiList)
        {
            chara.UpdateUi();
        }
    }
}