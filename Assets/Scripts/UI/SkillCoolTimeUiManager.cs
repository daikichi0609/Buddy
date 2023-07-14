using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class SkillCoolTimeUiManager : MonoBehaviour
{
    [Inject]
    private IUnitHolder m_UnitHolder;

    [SerializeField]
    private Text[] m_Texts;

    [SerializeField]
    private Image[] m_Images;

    [SerializeField]
    private Color32 m_CoolTimeWaitingColor;
    [SerializeField]
    private Color32 m_CoolTimeCompletedColor;
    [SerializeField]
    private Color32 m_CoolTimeInvalidColor;

    private void Update()
    {
        var leader = m_UnitHolder.Player;
        if (leader == null)
            return;

        var skillHandler = leader.GetInterface<ICharaSkillHandler>();

        for (int i = 0; i < m_Texts.Length; i++)
        {
            if (skillHandler.TryGetSkill(i, out var skill) == true)
            {
                int ct = skill.CurrentCoolTime;
                m_Texts[i].text = ct == 0 ? "OK" : ct.ToString();
                float rate = (float)(skill.MaxCoolTime - ct) / (float)(skill.MaxCoolTime);
                m_Images[i].fillAmount = rate;
                var color = rate == 1f ? m_CoolTimeCompletedColor : m_CoolTimeWaitingColor;
                m_Images[i].color = color;
            }
            else
            {
                m_Texts[i].text = string.Empty;
                m_Images[i].fillAmount = 1f;
                m_Images[i].color = m_CoolTimeInvalidColor;
            }
        }
    }
}
