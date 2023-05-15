using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System.Threading.Tasks;
using System;

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
    Task ActivateTrap(ICollector stepper, IUnitFinder unitFinder, AroundCell around, IDisposable disposable);

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
    private static readonly float OFFSET_Y = 0.5f;

    /// <summary>
    /// セットアップ
    /// </summary>
    private TrapSetup m_Setup;

    /// <summary>
    /// オブジェクト
    /// </summary>
    private GameObject m_GameObject;

    /// <summary>
    /// エフェクト
    /// </summary>
    private IEffectHandler m_Effect;

    /// <summary>
    /// 罠機能
    /// </summary>
    private ITrap m_Trap;
    bool ITrapHandler.HasTrap => m_Trap != null;

    /// <summary>
    /// 罠が見えているか
    /// </summary>
    private bool IsVisible { get; set; }
    bool ITrapHandler.IsVisible => IsVisible;

    /// <summary>
    /// 罠取得
    /// </summary>
    /// <param name="trap"></param>
    /// <returns></returns>
    async Task ITrapHandler.ActivateTrap(ICollector stepper, IUnitFinder unitFinder, AroundCell around, IDisposable disposable)
    {
        if (m_Trap == null)
        {
            Debug.LogAssertion("罠がありません");
            return;
        }

        var turn = stepper.GetInterface<ICharaTurn>();

        m_GameObject.SetActive(true);
        IsVisible = true;

        await m_Trap.Effect(m_Setup, stepper, unitFinder, around, m_Effect, m_GameObject.transform.position);
        disposable.Dispose();
    }

    /// <summary>
    /// 罠取得
    /// </summary>
    /// <param name="trap"></param>
    void ITrapHandler.SetTrap(TrapSetup setup)
    {
        m_Setup = setup;
        m_Trap = setup.TrapEffect;

        var pos = Owner.GetInterface<ICellInfoHandler>().Position;
        var v3 = pos + new Vector3(0f, OFFSET_Y, 0f);

        if (m_GameObject == null)
        {
            m_GameObject = ObjectPoolController.Interface.GetObject(m_Setup);
            m_GameObject.SetActive(false);
            var effect = Instantiate(m_Setup.EffectObject);
            m_Effect = new EffectHandler(effect);
        }
        m_GameObject.transform.position = v3;
    }

    protected override void Register(ICollector owner)
    {
        base.Register(owner);
        owner.Register<ITrapHandler>(this);
    }

    protected override void Dispose()
    {
        if (m_GameObject != null)
        {
            ObjectPoolController.Interface.SetObject(m_Setup, m_GameObject);
            m_Effect.Dispose();
        }

        base.Dispose();
    }
}