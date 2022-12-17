using UnityEngine;
using UnityEngine.UI;

public class CharaUi : MonoBehaviour
{
    private ICharaBattle m_Target;

    [SerializeField]
    private Text m_CharaName;

    [SerializeField]
    private Slider m_HpSlider;

    public void Initialize(ICollector target)
    {
        var battle = target.GetComponent<ICharaBattle>();
        m_Target = battle;
        m_CharaName.text = battle.Parameter.Name.ToString();
        m_HpSlider.maxValue = battle.Parameter.MaxHp;
        m_HpSlider.value = battle.Status.Hp;
    }

    public void UpdateUi()
    {
        m_HpSlider.value = m_Target.Status.Hp;
    }
}
