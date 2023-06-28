using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using NaughtyAttributes;
using Zenject;

public interface ICharaUiManager
{
    bool TryGetCharaUi(ICharaStatus status, out ICharaUi ui);
}

public class CharaUiManager : MonoBehaviour, ICharaUiManager
{
    [Inject]
    private IPlayerLoopManager m_LoopManager;
    [Inject]
    private IDungeonContentsDeployer m_DungeonContentsDeployer;
    [Inject]
    private IUnitHolder m_UnitHolder;

    /// <summary>
    /// キャンバス
    /// </summary>
    [SerializeField]
    private GameObject m_Canvas;

    /// <summary>
    /// プレハブ
    /// </summary>
    [SerializeField]
    private GameObject m_CharacterUiPrefab;

    /// <summary>
    /// 各キャラUIインスタンス格納用List
    /// </summary>
    private List<ICharaUi> m_CharacterUiList = new List<ICharaUi>();
    bool ICharaUiManager.TryGetCharaUi(ICharaStatus status, out ICharaUi ui)
    {
        foreach (var charaUi in m_CharacterUiList)
            if (charaUi.IsTarget(status) == true)
            {
                ui = charaUi;
                return true;
            }

        ui = null;
        return false;
    }

    private static readonly Vector3 ms_Diff = new Vector3(0f, -210f, 0f);

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
    void InitializeCharacterUi(ICollector[] units)
    {
        if (m_CharacterUiList.Count != 0)
            return;

        int i = 0;

        foreach (var unit in units)
        {
            GameObject obj = Instantiate(m_CharacterUiPrefab);
            obj.transform.SetParent(m_Canvas.transform, false);
            obj.transform.SetAsFirstSibling();
            ICharaUi ui = obj.GetComponent<CharaUi>();
            m_CharacterUiList.Add(ui);
            ui.Initialize(unit);

            obj.GetComponent<RectTransform>().transform.position += ms_Diff * i++;
        }
    }

    /// <summary>
    /// Ui更新
    /// </summary>
    private void UpdateCharaUi()
    {
        foreach (var chara in m_CharacterUiList)
            chara.UpdateUi();
    }
}