using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System.Threading.Tasks;
using DG.Tweening;
using Zenject;
using System;

public interface ICharaObjectHolder : IActorInterface
{
    /// <summary>
    /// 移動させるGameObject
    /// </summary>
    GameObject MoveObject { get; }

    /// <summary>
    /// キャラクターオブジェクト
    /// </summary>
    GameObject CharaObject { get; }

    /// <summary>
    /// 親子関係構築
    /// </summary>
    /// <param name="follow"></param>
    /// <returns></returns>
    IDisposable Follow(GameObject follow);

    /// <summary>
    /// 色登録
    /// </summary>
    /// <param name="color"></param>
    /// <returns></returns>
    IDisposable RegisterColor(Color32 color);
}

public class CharaObjectHolder : ActorComponentBase, ICharaObjectHolder
{
    [Inject]
    private IObjectPoolController m_ObjectPoolController;

    /// <summary>
    /// キャラのオブジェクト
    /// </summary>
    [SerializeField]
    private GameObject m_CharaObject;
    GameObject ICharaObjectHolder.CharaObject => m_CharaObject;

    /// <summary>
    /// 移動用オブジェクト
    /// </summary>
    [SerializeField]
    private GameObject m_MoveObject;
    GameObject ICharaObjectHolder.MoveObject => m_MoveObject;

    /// <summary>
    /// メッシュ
    /// </summary>
    [SerializeField]
    private SkinnedMeshRenderer m_MeshRenderer;
    private Color32 m_CurrentColor = DEFAULT_COLOR;
    IDisposable ICharaObjectHolder.RegisterColor(Color32 color)
    {
        m_CurrentColor = color;
        m_MeshRenderer.material.color = m_CurrentColor;
        return Disposable.CreateWithState((this, color), tuple =>
        {
            if (tuple.Item1.m_CurrentColor.IsSameColor(tuple.color) == true)
            {
                tuple.Item1.m_CurrentColor = DEFAULT_COLOR;
                tuple.Item1.m_MeshRenderer.material.color = tuple.Item1.m_CurrentColor;
            }
        });
    }

    private static readonly Color32 DEFAULT_COLOR = new Color32(255, 255, 255, 255);
    private static readonly Color32 RED_COLOR = new Color32(255, 108, 108, 255);
    private static readonly float FLASH_SPEED = 0.1f;

    protected override void Initialize()
    {
        base.Initialize();

        m_CharaObject.SetActive(true);

        if (Owner.RequireEvent<ICharaBattleEvent>(out var battle) == true)
        {
            battle.OnDamageStart.SubscribeWithState(this, async (result, self) =>
            {
                if (result.IsHit == true)
                    await self.RedFlash();
            }).AddTo(Owner.Disposables);
        }
    }

    protected override void Register(ICollector owner)
    {
        base.Register(owner);
        owner.Register<ICharaObjectHolder>(this);
    }

    protected override void Dispose()
    {
        if (Owner.RequireInterface<ICharaController>(out var _) == false)
        {
            var setup = Owner.GetInterface<ICharaStatus>().CurrentStatus.Setup;
            m_ObjectPoolController.SetObject(setup, m_MoveObject);
        }
        else
            Destroy(m_MoveObject);

        base.Dispose();
    }

    /// <summary>
    /// 赤点滅ダメージ演出
    /// </summary>
    /// <returns></returns>
    async private Task RedFlash()
    {
        await m_MeshRenderer.material.DOColor(RED_COLOR, FLASH_SPEED).AsyncWaitForCompletion();
        await m_MeshRenderer.material.DOColor(m_CurrentColor, FLASH_SPEED).AsyncWaitForCompletion();
        await m_MeshRenderer.material.DOColor(RED_COLOR, FLASH_SPEED).AsyncWaitForCompletion();
        await m_MeshRenderer.material.DOColor(m_CurrentColor, FLASH_SPEED).AsyncWaitForCompletion();
    }

    /// <summary>
    /// 追従させる
    /// </summary>
    /// <param name="follow"></param>
    /// <returns></returns>
    IDisposable ICharaObjectHolder.Follow(GameObject follow)
    {
        follow.transform.parent = m_MoveObject.gameObject.transform;
        return Disposable.CreateWithState(follow, follow => follow.transform.parent = null);
    }
}