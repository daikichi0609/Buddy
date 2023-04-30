using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Threading.Tasks;

public interface ICharaController
{
    /// <summary>
    /// 操作可能
    /// </summary>
    bool CanOperate { get; set; }

    /// <summary>
    /// 方向転換
    /// </summary>
    /// <param name="dest"></param>
    void Face(Vector3 dest);

    /// <summary>
    /// 移動
    /// </summary>
    /// <param name="dest"></param>
    Task Move(Vector3 dest, float duration);
}

public class CharaController : MonoBehaviour, ICharaController
{
    /// <summary>
    /// 動かすゲームオブジェクト
    /// </summary>
    [SerializeField]
    private GameObject m_MoveObject;

    /// <summary>
    /// アニメーター
    /// </summary>
    [SerializeField]
    private Animator m_CharaAnimator;

    /// <summary>
    /// 操作可能
    /// </summary>
    private bool m_CanOperate;
    bool ICharaController.CanOperate { get => m_CanOperate; set => m_CanOperate = value; }

    /// <summary>
    /// 向き直す
    /// </summary>
    /// <param name="dest"></param>
    private void Face(Vector3 dest)
    {
        m_MoveObject.transform.LookAt(dest);
    }
    void ICharaController.Face(Vector3 dest) => Face(dest);

    /// <summary>
    /// 定点移動　
    /// </summary>
    /// <param name="dest"></param>
    /// <param name="duration"></param>
    /// <returns></returns>
    async Task ICharaController.Move(Vector3 dest, float duration)
    {
        Face(dest);
        m_MoveObject.transform.DOMove(dest, duration);
        await PlayAnimation(ANIMATION_TYPE.MOVE, (int)duration * 1000);
    }

    /// <summary>
    /// 時間指定でモーション流す
    /// </summary>
    /// <param name="type"></param>
    /// <param name="time"></param>
    private async Task PlayAnimation(ANIMATION_TYPE type, int time)
    {
        PlayAnimation(type);
        await Task.Delay(time);
        StopAnimation(type);
    }

    /// <summary>
    /// モーション流す
    /// </summary>
    /// <param name="type"></param>
    private void PlayAnimation(ANIMATION_TYPE type) => m_CharaAnimator.SetBool(CharaAnimator.GetKey(type), true);

    /// <summary>
    /// モーション止める
    /// </summary>
    /// <param name="type"></param>
    private void StopAnimation(ANIMATION_TYPE type) => m_CharaAnimator.SetBool(CharaAnimator.GetKey(type), false);
}
