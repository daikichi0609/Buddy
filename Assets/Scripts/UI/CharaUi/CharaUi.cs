using System;
using Fungus;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public interface ICharaUi
{
    /// <summary>
    /// 対象
    /// </summary>
    bool IsTarget(ICharaStatus target);

    /// <summary>
    /// 初期化
    /// </summary>
    /// <param name="target"></param>
    void Initialize(ICollector target);

    /// <summary>
    /// UI更新
    /// </summary>
    void UpdateUi();

    /// <summary>
    /// 色変え
    /// </summary>
    /// <returns></returns>
    IDisposable ChangeBarColor(Color32 color);

    /// <summary>
    /// 表示切り替え
    /// </summary>
    /// <param name="isActive"></param>
    void SetActive(bool isActive);
}

public class CharaUi : MonoBehaviour, ICharaUi
{
    /// <summary>
    /// ターゲット
    /// </summary>
    private ICharaStatus m_Target;
    bool ICharaUi.IsTarget(ICharaStatus target) => target == m_Target;

    [SerializeField]
    private GameObject m_CharaUi;

    [SerializeField]
    private Text m_CharaName;

    [SerializeField]
    private Slider m_HpSlider;
    [SerializeField]
    private Image m_FillImage;

    [SerializeField]
    private Text m_HpValue;

    private bool m_IsChangingColor;

    private static readonly Color32 ms_Color1 = new Color(0, 255, 0, 255);
    private static readonly Color32 ms_Color2 = new Color(255, 255, 0, 255);
    private static readonly Color32 ms_Color3 = new Color(255, 182, 0, 255);
    private static readonly Color32 ms_Color4 = new Color(255, 0, 0, 255);

    void ICharaUi.Initialize(ICollector target)
    {
        var status = target.GetInterface<ICharaStatus>();
        m_Target = status;
    }

    void ICharaUi.UpdateUi()
    {
        if (m_Target == null || m_Target.CurrentStatus.OriginParam == null)
        {
#if DEBUG
            Debug.Log("パラメタがnullです");
#endif
            return;
        }

        m_CharaName.text = m_Target.CurrentStatus.OriginParam.GivenName.ToString();

        int maxHp = m_Target.CurrentStatus.MaxHp;
        int hp = m_Target.CurrentStatus.Hp;
        m_HpSlider.maxValue = maxHp;
        m_HpSlider.value = hp;
        m_HpValue.text = hp + " / " + maxHp;

        if (m_IsChangingColor == false)
        {
            float ratio = (float)hp / (float)maxHp;

            if (ratio > 0.55f)
            {
                m_FillImage.color = Color.Lerp(ms_Color2, ms_Color1, (ratio - 0.55f) * 4f);
            }
            else if (ratio > 0.25f)
            {
                m_FillImage.color = Color.Lerp(ms_Color3, ms_Color2, (ratio - 0.25f) * 4f);
            }
            else
            {
                m_FillImage.color = Color.Lerp(ms_Color4, ms_Color3, ratio * 4f);
            }
        }
    }

    IDisposable ICharaUi.ChangeBarColor(Color32 color)
    {
        m_IsChangingColor = true;
        m_FillImage.color = color;
        return Disposable.CreateWithState(this, self => self.m_IsChangingColor = false);
    }

    void ICharaUi.SetActive(bool isActive) => m_CharaUi.SetActive(isActive);
}
