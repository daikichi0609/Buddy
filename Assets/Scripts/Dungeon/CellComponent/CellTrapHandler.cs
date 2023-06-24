using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System.Threading.Tasks;
using System;
using Zenject;
using UniRx;

public interface ITrapHandler : IActorInterface
{
    /// <summary>
    /// 罠があるかどうか
    /// </summary>
    /// <returns></returns>
    bool HasTrap { get; }

    /// <summary>
    /// 罠作動、あるなら
    /// </summary>
    /// <param name="trap"></param>
    /// <returns></returns>
    Task ActivateTrap(ICollector stepper, IUnitFinder unitFinder, IDungeonHandler dungeonHandler, IBattleLogManager battleLogManager, IDisposable disposable);

    /// <summary>
    /// 罠設置
    /// </summary>
    /// <param name="trap"></param>
    void SetTrap(TrapSetup setup);

    /// <summary>
    /// 罠が見えるかどうか
    /// </summary>
    bool IsVisible { get; }
}

public class CellTrapHandler : ActorComponentBase, ITrapHandler
{
    [Inject]
    private IObjectPoolController m_ObjectPoolController;
    [Inject]
    private IMiniMapRenderer m_MiniMapRenderer;

    private static readonly float OFFSET_Y = 0.5f;

    /// <summary>
    /// セットアップ
    /// </summary>
    private TrapSetup m_Setup;
    bool ITrapHandler.HasTrap => m_Setup != null;

    /// <summary>
    /// オブジェクト
    /// </summary>
    private GameObject m_GameObject;

    /// <summary>
    /// エフェクト
    /// </summary>
    private IEffectHandler m_Effect = new EffectHandler();

    /// <summary>
    /// 罠が見えているか
    /// </summary>
    private ReactiveProperty<bool> m_IsVisible = new ReactiveProperty<bool>();
    bool ITrapHandler.IsVisible => m_IsVisible.Value;

    protected override void Initialize()
    {
        base.Initialize();

        m_IsVisible.SubscribeWithState(this, (isVisible, self) =>
        {
            if (isVisible == true)
            {
                var disposable = self.m_MiniMapRenderer.RegisterIcon(self.Owner);
                self.Owner.Disposables.Add(disposable);
            }
        }).AddTo(Owner.Disposables);
    }

    /// <summary>
    /// 罠作動
    /// </summary>
    /// <param name="trap"></param>
    /// <returns></returns>
    async Task ITrapHandler.ActivateTrap(ICollector stepper, IUnitFinder unitFinder, IDungeonHandler dungeonHandler, IBattleLogManager battleLogManager, IDisposable disposable)
    {
        if (m_Setup == null)
        {
            Debug.LogWarning("罠がありません");
            return;
        }

        m_GameObject.SetActive(true);
        m_IsVisible.Value = true;

        await m_Setup.TrapEffect.Effect(m_Setup, stepper, Owner, unitFinder, dungeonHandler, battleLogManager, m_Effect, m_GameObject.transform.position);
        disposable.Dispose();
    }

    /// <summary>
    /// 罠取得
    /// </summary>
    /// <param name="trap"></param>
    void ITrapHandler.SetTrap(TrapSetup setup)
    {
        m_Setup = setup;

        var pos = Owner.GetInterface<ICellInfoHandler>().Position;
        var v3 = pos + new Vector3(0f, OFFSET_Y, 0f);

        m_GameObject = m_ObjectPoolController.GetObject(m_Setup);
        m_GameObject.transform.position = v3;
        m_GameObject.SetActive(false);

        var effect = Instantiate(m_Setup.EffectObject);
        m_Effect.SetEffect(effect);
    }

    protected override void Register(ICollector owner)
    {
        base.Register(owner);
        owner.Register<ITrapHandler>(this);
    }

    protected override void Dispose()
    {
        if (m_Setup != null)
        {
            m_ObjectPoolController.SetObject(m_Setup, m_GameObject);
            m_Effect.Dispose();
            m_Setup = null;
        }

        base.Dispose();
    }
}