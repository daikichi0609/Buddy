using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public readonly struct BattleUiSwitch
{
    public bool Switch { get; }

    public BattleUiSwitch(bool s) => Switch = s;
}

public class TeamLevelUiManager : MonoBehaviour
{
    [Inject]
    private ITeamLevelHandler m_TeamLevelHandler;

    [SerializeField]
    private Text m_Leveltext;

    [SerializeField]
    private Slider m_ExpSlider;

    private void Awake()
    {
        MessageBroker.Default.Receive<BattleUiSwitch>().Subscribe(s =>
        {
            m_Leveltext.gameObject.SetActive(s.Switch);
            m_ExpSlider.gameObject.SetActive(s.Switch);
        }).AddTo(this);
    }

    private void Update()
    {
        m_Leveltext.text = "Lv " + m_TeamLevelHandler.Level.ToString();
        m_ExpSlider.maxValue = 1;
        m_ExpSlider.value = m_TeamLevelHandler.ExpFillAmount;
    }
}
