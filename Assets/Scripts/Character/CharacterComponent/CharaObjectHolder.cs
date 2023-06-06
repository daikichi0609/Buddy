using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System.Threading.Tasks;
using DG.Tweening;
using Zenject;

public interface ICharaObjectHolder : IActorInterface
{
    GameObject MoveObject { get; }

    GameObject CharaObject { get; }
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

    private static readonly Color DEFAULT_COLOR = new Color(1f, 1f, 1f, 1f);
    private static readonly Color RED_COLOR = new Color(1f, 0.4f, 0.4f, 1f);
    private static readonly float FLASH_SPEED = 0.1f;

    protected override void Initialize()
    {
        base.Initialize();

        m_CharaObject.SetActive(true);

        if (Owner.RequireEvent<ICharaBattleEvent>(out var battle) == true)
        {
            battle.OnDamageStart.Subscribe(async _ => await RedFlash()).AddTo(CompositeDisposable);
        }
    }

    protected override void Register(ICollector owner)
    {
        base.Register(owner);
        owner.Register<ICharaObjectHolder>(this);
    }

    protected override void Dispose()
    {
        var setup = Owner.GetInterface<ICharaStatus>().Setup;
        m_ObjectPoolController.SetObject(setup, m_MoveObject);
        base.Dispose();
    }

    /// <summary>
    /// 赤点滅ダメージ演出
    /// </summary>
    /// <returns></returns>
    async private Task RedFlash()
    {
        m_MeshRenderer.material.DOColor(RED_COLOR, FLASH_SPEED);
        await Task.Delay(100);
        m_MeshRenderer.material.DOColor(DEFAULT_COLOR, FLASH_SPEED);
        await Task.Delay(100);
        m_MeshRenderer.material.DOColor(RED_COLOR, FLASH_SPEED);
        await Task.Delay(100);
        m_MeshRenderer.material.DOColor(DEFAULT_COLOR, FLASH_SPEED);
    }
}