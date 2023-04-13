using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System.Threading.Tasks;

public interface ITrapHandler : IActorInterface
{
    /// <summary>
    /// 罠取得、あるなら
    /// </summary>
    /// <param name="trap"></param>
    /// <returns></returns>
    Task<bool> ActivateTrap(ICollector stepper, IUnitFinder unitFinder, AroundCell around);

    /// <summary>
    /// 罠設置
    /// </summary>
    /// <param name="trap"></param>
    void SetTrap(TrapSetup setup, ITrap trap, Vector3Int pos);
}

public class TrapHandler : ActorComponentBase, ITrapHandler
{
    /// <summary>
    /// セットアップ
    /// </summary>
    private TrapSetup m_Setup;

    /// <summary>
    /// オブジェクト
    /// </summary>
    private GameObject m_GameObject;

    /// <summary>
    /// 罠機能
    /// </summary>
    [SerializeField, ReadOnly, Expandable]
    private ITrap m_Trap;

    /// <summary>
    /// 罠取得
    /// </summary>
    /// <param name="trap"></param>
    /// <returns></returns>
    async Task<bool> ITrapHandler.ActivateTrap(ICollector stepper, IUnitFinder unitFinder, AroundCell around)
    {
        if (m_Trap == null)
            return false;

        m_GameObject.SetActive(true);
        await m_Trap.Effect(m_Setup, stepper, unitFinder, around);
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

        if (m_GameObject == null)
        {
            m_GameObject = Instantiate(m_Setup.Prefab);
            m_GameObject.SetActive(false);
        }
        m_GameObject.transform.position = pos + new Vector3(0f, 0.5f, 0f);
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