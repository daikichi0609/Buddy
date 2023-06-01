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
        m_HpSlider.maxValue = m_Target.CurrentStatus.OriginParam.MaxHp;
        m_HpSlider.value = m_Target.CurrentStatus.Hp;
    }
}
