using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using NaughtyAttributes;
using Zenject;

public interface ICharaUiManager
{
    void InitializeCharacterUi(ICollector[] units);
}

public class CharaUiManager : MonoBehaviour, ICharaUiManager
{
    [Inject]
    private IPlayerLoopManager m_LoopManager;
    [Inject]
    private IDungeonContentsDeployer m_DungeonContentsDeployer;
    [Inject]
    private IUnitHolder m_UnitHolder;

    //キャンバス
    [SerializeField]
    private GameObject m_Canvas;

    //キャラクターUIプレハブ
    [SerializeField]
    private GameObject m_CharacterUiPrefab;

    private static readonly Vector3 ms_Diff = new Vector3(0f, -210f, 0f);

    //各キャラUI格納用List（キャラUiは他とは別）
    [SerializeField, ReadOnly]
    private List<CharaUi> m_CharacterUiList = new List<CharaUi>();
    public List<CharaUi> CharacterUiList
    {
        get { return m_CharacterUiList; }
        set { m_CharacterUiList = value; }
    }

    private void Awake()
    {
        // Ui初期化
        m_DungeonContentsDeployer.OnDeployContents.SubscribeWithState(this, (_, self) =>
        {
            var units = self.m_UnitHolder.FriendList.ToArray();
            self.InitializeCharacterUi(units);
        });

        // Updateで更新
        m_LoopManager.GetUpdateEvent
            .SubscribeWithState(this, (_, self) => self.UpdateCharaUi()).AddTo(this);
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
            GameObject obj = Instantiate(m_CharacterUiPrefab);
            obj.transform.SetParent(m_Canvas.transform, false);
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