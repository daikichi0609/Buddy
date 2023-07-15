using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Threading.Tasks;
using UniRx;
using NaughtyAttributes;
using System;

public interface ICharaController : IActorInterface
{
    /// <summary>
    /// 座標
    /// </summary>
    Vector3 Position { get; }

    /// <summary>
    /// Rigidbody
    /// </summary>
    Rigidbody Rigidbody { get; }

    /// <summary>
    /// 方向転換
    /// </summary>
    /// <param name="dest"></param>
    void Face(Vector3 dest);
    void Face(DIRECTION dir);

    /// <summary>
    /// 定点移動
    /// </summary>
    /// <param name="dest"></param>
    Task MoveToPoint(Vector3 dest, float duration);

    /// <summary>
    /// ワープ
    /// </summary>
    /// <param name="pos"></param>
    void Wrap(Vector3 pos);

    /// <summary>
    /// 移動
    /// </summary>
    /// <param name="dir"></param>
    void Move(DIRECTION dir);

    /// <summary>
    /// アニメーションキャンセル
    /// </summary>
    void StopAnimation();
}

public partial class CharaController : ActorComponentBase, ICharaController
{
    private ICharaObjectHolder m_CharaObjectHolder;
    private ICharaAnimator m_CharaAnimator;

    /// <summary>
    /// スピード
    /// </summary>
    private static readonly float ms_Speed = 3f;

    /// <summary>
    /// 座標
    /// </summary>
    [ShowNativeProperty]
    private Vector3 Position => m_CharaObjectHolder.MoveObject.transform.position;
    Vector3 ICharaController.Position => Position;

    /// <summary>
    /// Rigidbody
    /// </summary>
    private Rigidbody m_RigidBody;
    Rigidbody ICharaController.Rigidbody => m_RigidBody;

    protected override void Register(ICollector owner)
    {
        base.Register(owner);
        owner.Register<ICharaController>(this);
    }

    protected override void Initialize()
    {
        base.Initialize();
        m_CharaObjectHolder = Owner.GetInterface<ICharaObjectHolder>();
        m_CharaAnimator = Owner.GetInterface<ICharaAnimator>();
        m_RigidBody = m_CharaObjectHolder.MoveObject.GetComponent<Rigidbody>();
    }

    /// <summary>
    /// 向き直す
    /// </summary>
    /// <param name="lookPos"></param>
    private void Face(Vector3 lookPos) => m_CharaObjectHolder.CharaObject.transform.rotation = Quaternion.LookRotation(lookPos);
    void ICharaController.Face(DIRECTION dir) => m_CharaObjectHolder.CharaObject.transform.rotation = Quaternion.LookRotation(dir.ToV3Int());
    void ICharaController.Face(Vector3 lookPos)
    {
        var dir = lookPos - Position;
        Face(new Vector3(dir.x, 0f, dir.z));
    }

    /// <summary>
    /// 定点移動　
    /// </summary>
    /// <param name="dest"></param>
    /// <param name="duration"></param>
    /// <returns></returns>
    async Task ICharaController.MoveToPoint(Vector3 dest, float duration)
    {
        // 移動方向を向く
        var dir = dest - m_CharaObjectHolder.MoveObject.transform.position;
        Face(dir);

        // 定点移動
        var anim = m_CharaAnimator.PlayAnimation(ANIMATION_TYPE.MOVE);
        await m_CharaObjectHolder.MoveObject.transform.DOMove(dest, duration).SetEase(Ease.Linear).AsyncWaitForCompletion();
        anim.Dispose();
    }

    /// <summary>
    /// ワープ
    /// </summary>
    /// <param name="pos"></param>
    void ICharaController.Wrap(Vector3 pos) => m_CharaObjectHolder.MoveObject.transform.position = pos;
}


public partial class CharaController
{
    private IDisposable m_StopMoving;

    /// <summary>
    /// プレイヤー操作による移動
    /// </summary>
    /// <param name="dir"></param>
    void ICharaController.Move(DIRECTION dir)
    {
        // 移動方向を向く
        Face(dir.ToV3Int());

        // アニメーション開始
        if (m_StopMoving == null)
            m_StopMoving = m_CharaAnimator.PlayAnimation(ANIMATION_TYPE.MOVE);

        // 移動
        m_CharaObjectHolder.MoveObject.transform.position += (Vector3)dir.ToV3Int() * ms_Speed * Time.deltaTime;
    }

    void ICharaController.StopAnimation()
    {
        m_StopMoving?.Dispose();
        m_StopMoving = null;
    }
}