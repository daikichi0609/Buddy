using UnityEngine;
using UnityEngine.UI;

public class CharaUi : MonoBehaviour
{
    private ICharaStatus m_Target;

    [SerializeField]
    private Text m_CharaName;

    [SerializeField]
    private Slider m_HpSlider;

    public void Initialize(ICollector target)
    {
        var status = target.GetComponent<ICharaStatus>();
        m_Target = status;
        m_CharaName.text = status.Parameter.GivenName.ToString();
        m_HpSlider.maxValue = status.Parameter.MaxHp;
        m_HpSlider.value = status.CurrentStatus.Hp;
    }

    public void UpdateUi()
    {
        m_HpSlider.value = m_Target.CurrentStatus.Hp;
    }
}
