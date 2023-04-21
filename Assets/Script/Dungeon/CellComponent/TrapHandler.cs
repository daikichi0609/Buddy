using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System.Threading.Tasks;

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
    bool ActivateTrap(ICollector stepper, IUnitFinder unitFinder, AroundCell around);

    /// <summary>
    /// 罠設置
    /// </summary>
    /// <param name="trap"></param>
    void SetTrap(TrapSetup setup, ITrap trap, Vector3Int pos);

    /// <summary>
    /// 罠が見えるかどうか
    /// </summary>
    bool IsVisible { get; }
}

public class TrapHandler : ActorComponentBase, ITrapHandler
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
    private EffectHadler m_Effect;

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
    bool ITrapHandler.ActivateTrap(ICollector stepper, IUnitFinder unitFinder, AroundCell around)
    {
        if (m_Trap == null)
            return false;

        var disposable = TurnManager.Interface.RequestProhibitAction();

        var turn = stepper.GetInterface<ICharaTurn>();

        m_GameObject.SetActive(true);
        IsVisible = true;

        Task.Run(() => m_Trap.Effect(m_Setup, stepper, unitFinder, around, m_Effect, m_GameObject.transform.position));
        disposable.Dispose();
        return true;
    }

    /// <summary>
    /// 罠取得
    /// </summary>
    /// <param name="trap"></param>
    void ITrapHandler.SetTrap(TrapSetup setup, ITrap trap, Vector3Int pos)
    {
        m_Setup = setup;
        m_Trap = trap;

        var v3 = pos + new Vector3(0f, OFFSET_Y, 0f);

        if (m_GameObject == null)
        {
            m_GameObject = Instantiate(m_Setup.Prefab);
            m_GameObject.SetActive(false);
            var effect = Instantiate(m_Setup.Effect);
            m_Effect = new EffectHadler(effect);
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
        m_GameObject.SetActive(false);
        base.Dispose();
    }
}