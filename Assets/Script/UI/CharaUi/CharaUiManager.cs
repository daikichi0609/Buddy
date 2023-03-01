using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public interface ICharaUiManager : ISingleton
{
    void InitializeCharacterUi(ICollector player);
}

public class CharaUiManager : Singleton<CharaUiManager, ICharaUiManager>, ICharaUiManager
{
    //各キャラUI格納用List（キャラUiは他とは別）
    [SerializeField] private List<CharaUi> m_CharacterUiList = new List<CharaUi>();
    public List<CharaUi> CharacterUiList
    {
        get { return m_CharacterUiList; }
        set { m_CharacterUiList = value; }
    }

    protected override void Awake()
    {
        base.Awake();

        GameManager.Interface.GetUpdateEvent
            .Subscribe(_ => UpdateCharaUi()).AddTo(this);
    }

    void ICharaUiManager.InitializeCharacterUi(ICollector player)
    {
        GameObject obj = Instantiate(UiHolder.Instance.CharacterUi);
        obj.transform.SetParent(UiHolder.Instance.Canvas.transform, false);
        CharaUi ui = obj.GetComponent<CharaUi>();
        CharacterUiList.Add(ui);
        ui.Initialize(player);
    }

    public void UpdateCharaUi()
    {
        foreach (var chara in CharacterUiList)
        {
            chara.UpdateUi();
        }
    }
}