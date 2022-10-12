using UnityEngine;
using UnityEngine.UI;

public class CharaUi : MonoBehaviour
{
    [SerializeField] private GameObject m_TargetObject;
    public GameObject TargetObject
    {
        get { return m_TargetObject; }
        set { m_TargetObject = value; }
    }

    [SerializeField] private Text m_CharaText;
    public Text CharaName
    {
        get { return m_CharaText; }
        set { m_CharaText = value; }
    }

    [SerializeField] private Slider m_HpSlider;
    public Slider HpSlider
    {
        get { return m_HpSlider; }
        set { m_HpSlider = value; }
    }

    public void Initialize(GameObject target)
    {
        TargetObject = target;
        CharaBattle chara = TargetObject.GetComponent<CharaBattle>();
        CharaName.text = chara.Parameter.Name.ToString();
        HpSlider.maxValue = chara.MaxHp;
        HpSlider.value = chara.Parameter.Hp;
    }

    public void UpdateUi()
    {
        BattleStatus.Parameter param = TargetObject.GetComponent<CharaBattle>().Parameter;
        HpSlider.value = param.Hp;
    }
}
