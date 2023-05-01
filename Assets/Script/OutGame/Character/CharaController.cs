using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Threading.Tasks;
using UniRx;

/// <summary>
/// ステート
/// </summary>
public enum CHARA_STATE
{
    IDLE,
    MOVE,
    TALK,
}

public interface ICharaController : IActorInterface
{
    /// <summary>
    /// 座標
    /// </summary>
    Vector3 Position { get; }

    /// <summary>
    /// 方向転換
    /// </summary>
    /// <param name="dest"></param>
    void Face(Vector3 dest);

    /// <summary>
    /// 定点移動
    /// </summary>
    /// <param name="dest"></param>
    Task Move(Vector3 dest, float duration);

    /// <summary>
    /// ワープ
    /// </summary>
    /// <param name="pos"></param>
    void Wrap(Vector3 pos);

    /// <summary>
    /// 移動
    /// </summary>
    /// <param name="dir"></param>
    void Move(Vector3 dir);
}

public partial class CharaController : ActorComponentBase, ICharaController
{
    /// <summary>
    /// スピード
    /// </summary>
    [SerializeField]
    private float m_Speed;

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
    /// 座標
    /// </summary>
    Vector3 ICharaController.Position => m_MoveObject.transform.position;

    protected override void Register(ICollector owner)
    {
        base.Register(owner);
        owner.Register<ICharaController>(this);
    }

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
        // 移動方向を向く
        Face(dest);

        // 定点移動
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

    /// <summary>
    /// ワープ
    /// </summary>
    /// <param name="pos"></param>
    void ICharaController.Wrap(Vector3 pos) => m_MoveObject.transform.position = pos;
}


public partial class CharaController
{
    /// <summary>
    /// プレイヤー操作による移動
    /// </summary>
    /// <param name="dir"></param>
    void ICharaController.Move(Vector3 dir)
    {
        // 移動方向を向く
        Face(dir.ToV3Int());

        // アニメーション開始
        PlayAnimation(ANIMATION_TYPE.MOVE);

        // 移動
        m_MoveObject.transform.position += dir * m_Speed * Time.deltaTime;
    }
}