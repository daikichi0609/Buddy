using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using DG.Tweening;

public interface IAttackResultUiManager
{
    void Damage(AttackResult result);
    void Miss(AttackResult result);
}

public class AttackResultUiManager : MonoBehaviour, IAttackResultUiManager
{
    // フェイド速度
    private static readonly float FADE_SPEED = 1f;
    // 表示位置
    private static readonly Vector3 OFFSET = new Vector3(0f, 1.5f, 0f);
    // 移動距離
    private static readonly float DISTANCE = 0.5f;

    /// <summary>
    /// ダメージテキスト
    /// </summary>
    [SerializeField]
    private Text m_DamageText;

    /// <summary>
    /// Missテキスト
    /// </summary>
    [SerializeField]
    private Text m_MissText;

    /// <summary>
    /// ダメージ表示
    /// </summary>
    /// <param name="result"></param>
    void IAttackResultUiManager.Damage(AttackResult result)
    {
        // 透明度操作 //
        string damage = result.Damage.ToString();
        m_DamageText.text = damage;
        m_DamageText.DOFade(1f, 0.001f);
        m_DamageText.DOFade(0f, FADE_SPEED);

        // 位置操作
        var defender = result.Defender;
        var move = defender.GetInterface<ICharaMove>();
        m_DamageText.transform.position = move.Position + OFFSET;
        m_DamageText.transform.DOLocalMove(new Vector3(0f, DISTANCE, 0f), FADE_SPEED).SetRelative(true);
    }

    /// <summary>
    /// Miss表示
    /// </summary>
    /// <param name="result"></param>
    void IAttackResultUiManager.Miss(AttackResult result)
    {
        // 透明度操作 //
        m_MissText.DOFade(1f, 0.001f);
        m_MissText.DOFade(0f, FADE_SPEED);

        // 位置操作
        var defender = result.Defender;
        var move = defender.GetInterface<ICharaMove>();
        m_MissText.transform.position = move.Position + OFFSET;
        m_MissText.transform.DOLocalMove(new Vector3(0f, DISTANCE, 0f), FADE_SPEED).SetRelative(true);
    }
}