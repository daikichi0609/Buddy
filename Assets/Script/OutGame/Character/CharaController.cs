using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Threading.Tasks;
using UniRx;
using NaughtyAttributes;

public interface ICharaController : IActorInterface
{
    /// <summary>
    /// GameObject
    /// </summary>
    GameObject MoveObject { get; }

    /// <summary>
    /// 座標
    /// </summary>
    Vector3 Position { get; }

    /// <summary>
    /// Rigidbody
    /// </summary>
    Rigidbody Rigidbody { get; }

    /// <summary>
    /// アニメーションキャンセル
    /// </summary>
    void StopAnimation(ANIMATION_TYPE type);

    /// <summary>
    /// 方向転換
    /// </summary>
    /// <param name="dest"></param>
    void Face(Vector3 dest);

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
}

public partial class CharaController : ActorComponentBase, ICharaController
{
    /// <summary>
    /// スピード
    /// </summary>
    private static readonly float ms_Speed = 3f;

    /// <summary>
    /// 動かすゲームオブジェクト
    /// </summary>
    [SerializeField]
    private GameObject m_MoveObject;
    GameObject ICharaController.MoveObject => m_MoveObject;

    /// <summary>
    /// キャラ
    /// </summary>
    [SerializeField]
    private GameObject m_CharaObject;

    /// <summary>
    /// アニメーター
    /// </summary>
    [SerializeField]
    private Animator m_CharaAnimator;

    /// <summary>
    /// 座標
    /// </summary>
    [ShowNativeProperty]
    Vector3 ICharaController.Position => m_MoveObject.transform.position;

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
        m_RigidBody = m_MoveObject.GetComponent<Rigidbody>();
    }

    /// <summary>
    /// 向き直す
    /// </summary>
    /// <param name="dir"></param>
    private void Face(Vector3 dir) => m_CharaObject.transform.rotation = Quaternion.LookRotation(dir);
    void ICharaController.Face(Vector3 dest) => Face(dest);

    /// <summary>
    /// 定点移動　
    /// </summary>
    /// <param name="dest"></param>
    /// <param name="duration"></param>
    /// <returns></returns>
    async Task ICharaController.MoveToPoint(Vector3 dest, float duration)
    {
        // 移動方向を向く
        var dir = dest - m_MoveObject.transform.position;
        Face(dir);

        // 定点移動
        m_MoveObject.transform.DOMove(dest, duration).SetEase(Ease.Linear);
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
    void ICharaController.StopAnimation(ANIMATION_TYPE type) => StopAnimation(type);

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
    void ICharaController.Move(DIRECTION dir)
    {
        // 移動方向を向く
        Face(dir.ToV3Int());

        // アニメーション開始
        PlayAnimation(ANIMATION_TYPE.MOVE);

        // 移動
        m_MoveObject.transform.position += (Vector3)dir.ToV3Int() * ms_Speed * Time.deltaTime;
    }
}