using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class CharaUiManager : SingletonMonoBehaviour<CharaUiManager>
{
    //各キャラUI格納用List（キャラUiは他とは別）
    [SerializeField] private List<GameObject> m_CharacterUiList = new List<GameObject>();
    public List<GameObject> CharacterUiList
    {
        get { return m_CharacterUiList; }
        set { m_CharacterUiList = value; }
    }

    protected override void Awake()
    {
        base.Awake();

        GameManager.Instance.GetUpdate
            .Subscribe(_ => UpdateCharaUi());
    }

    public void GenerateCharacterUi(GameObject player)
    {
        GameObject ui = Instantiate(UiHolder.Instance.CharacterUi);
        ui.transform.SetParent(UiHolder.Instance.Canvas.transform, false);
        CharacterUiList.Add(ui);
        CharaUi charaUi = ui.GetComponent<CharaUi>();
        charaUi.Initialize(player);
    }

    public void UpdateCharaUi()
    {
        foreach (GameObject chara in CharacterUiList)
        {
            chara.GetComponent<CharaUi>().UpdateUi();
        }
    }
}