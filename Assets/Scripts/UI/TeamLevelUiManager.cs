using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class TeamLevelUiManager : MonoBehaviour
{
    [Inject]
    private ITeamLevelHandler m_TeamLevelHandler;

    [SerializeField]
    private Text m_Leveltext;

    [SerializeField]
    private Slider m_ExpSlider;

    private void Update()
    {
        m_Leveltext.text = "Lv " + m_TeamLevelHandler.Level.ToString();
        m_ExpSlider.maxValue = 1;
        m_ExpSlider.value = m_TeamLevelHandler.ExpFillAmount;
    }
}
