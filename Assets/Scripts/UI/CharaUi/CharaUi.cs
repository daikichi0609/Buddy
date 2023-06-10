using UnityEngine;
using UnityEngine.UI;

public class CharaUi : MonoBehaviour
{
    private ICharaStatus m_Target;

    [SerializeField]
    private Text m_CharaName;

    [SerializeField]
    private Slider m_HpSlider;

    [SerializeField]
    private Text m_HpValue;

    public void Initialize(ICollector target)
    {
        var status = target.GetInterface<ICharaStatus>();
        m_Target = status;
    }

    public void UpdateUi()
    {
        if (m_Target == null || m_Target.CurrentStatus.OriginParam == null)
        {
#if DEBUG
            Debug.Log("パラメタがnullです");
#endif
            return;
        }

        m_CharaName.text = m_Target.CurrentStatus.OriginParam.GivenName.ToString();

        int maxHp = m_Target.CurrentStatus.OriginParam.MaxHp;
        int hp = m_Target.CurrentStatus.Hp;
        m_HpSlider.maxValue = maxHp;
        m_HpSlider.value = hp;
        m_HpValue.text = hp + " / " + maxHp;
    }
}
