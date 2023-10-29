using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using DG.Tweening;
using Zenject;
using System;

public interface IPopUpUiManager
{
    void Damage(AttackResult result);
    void Miss(AttackResult result);
    void LevelUp(ICollector unit);
}

public class PopUpUiManager : MonoBehaviour, IPopUpUiManager
{
    // フェイド速度
    private static readonly float FADE_SPEED = 1.0f;
    // 表示位置
    private static readonly Vector3 OFFSET = new Vector3(0f, 1.5f, 0f);
    // 移動距離
    private static readonly float DISTANCE = 0.5f;

    /// <summary>
    /// ダメージテキスト
    /// </summary>
    [SerializeField]
    private Text[] m_DamageText;
    private bool m_DamageTextIndexFlag;
    private Text DamageText => m_DamageText[Convert.ToInt32(m_DamageTextIndexFlag)];

    /// <summary>
    /// クリティカルテキスト
    /// </summary>
    [SerializeField]
    private Text[] m_CriticalText;
    private bool m_CriticalTextIndexFlag;
    private Text CriticalText => m_CriticalText[Convert.ToInt32(m_CriticalTextIndexFlag)];

    /// <summary>
    /// Missテキスト
    /// </summary>
    [SerializeField]
    private Text[] m_MissText;
    private bool m_MissTextIndexFlag;
    private Text MissText => m_MissText[Convert.ToInt32(m_MissTextIndexFlag)];

    /// <summary>
    /// レベルアップテキスト
    /// </summary>
    [SerializeField]
    private Text m_LevelUpText;

    /// <summary>
    /// ダメージ表示
    /// </summary>
    /// <param name="result"></param>
    void IPopUpUiManager.Damage(AttackResult result)
    {
        // 透明度操作
        string damage = result.Damage.ToString();
        DamageText.text = damage;
        DamageText.DOFade(1f, 0.001f);
        DamageText.DOFade(0f, FADE_SPEED);

        // 位置操作
        var defender = result.Defender;
        var move = defender.GetInterface<ICharaMove>();
        DamageText.transform.position = move.Position + OFFSET;
        DamageText.transform.DOLocalMove(new Vector3(0f, DISTANCE, 0f), FADE_SPEED).SetRelative(true);
        m_DamageTextIndexFlag = !m_DamageTextIndexFlag;

        if (result.IsCritical == true)
        {
            CriticalText.DOFade(1f, 0.001f);
            CriticalText.DOFade(0f, FADE_SPEED);
            CriticalText.transform.position = move.Position + OFFSET + new Vector3(0f, 0.5f, 0f);
            CriticalText.transform.DOLocalMove(new Vector3(0f, DISTANCE, 0f), FADE_SPEED).SetRelative(true);
            m_CriticalTextIndexFlag = !m_CriticalTextIndexFlag;
        }
    }

    /// <summary>
    /// Miss表示
    /// </summary>
    /// <param name="result"></param>
    void IPopUpUiManager.Miss(AttackResult result)
    {
        // 透明度操作
        MissText.DOFade(1f, 0.001f);
        MissText.DOFade(0f, FADE_SPEED);

        // 位置操作
        var defender = result.Defender;
        var move = defender.GetInterface<ICharaMove>();
        MissText.transform.position = move.Position + OFFSET;
        MissText.transform.DOLocalMove(new Vector3(0f, DISTANCE, 0f), FADE_SPEED).SetRelative(true);
        m_MissTextIndexFlag = !m_MissTextIndexFlag;
    }

    /// <summary>
    /// レベルアップ
    /// </summary>
    /// <param name="unit"></param>
    void IPopUpUiManager.LevelUp(ICollector unit)
    {
        // 透明度操作
        m_LevelUpText.DOFade(1f, 0.001f);
        m_LevelUpText.DOFade(0f, FADE_SPEED);

        // 位置操作
        var move = unit.GetInterface<ICharaMove>();
        m_LevelUpText.transform.position = move.Position + OFFSET;
        m_LevelUpText.transform.DOLocalMove(new Vector3(0f, DISTANCE, 0f), FADE_SPEED).SetRelative(true);
    }
}