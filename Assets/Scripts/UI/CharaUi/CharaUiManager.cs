using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using NaughtyAttributes;
using Zenject;
using System;

public interface ICharaUiManager
{
    /// <summary>
    /// 表示切り替え
    /// </summary>
    /// <param name="isActive"></param>
    /// <returns></returns>
    IDisposable SetActive(bool isActive);

    /// <summary>
    /// Ui取得
    /// </summary>
    /// <param name="status"></param>
    /// <param name="ui"></param>
    /// <returns></returns>
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

    private static readonly Vector3 ms_Diff = new Vector3(0f, -210f, 0f);

    /// <summary>
    /// キャンバス
    /// </summary>
    [SerializeField]
    private GameObject m_ParentObject;

    IDisposable ICharaUiManager.SetActive(bool isActive)
    {
        foreach (var ui in m_CharacterUiList)
            ui.SetActive(isActive);

        return Disposable.CreateWithState((this, isActive), tuple =>
        {
            foreach (var ui in tuple.Item1.m_CharacterUiList)
                ui.SetActive(!tuple.isActive);
        });
    }

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
            obj.transform.SetParent(m_ParentObject.transform, false);
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