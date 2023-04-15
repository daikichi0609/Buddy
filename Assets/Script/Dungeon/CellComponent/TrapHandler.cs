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

        var turn = stepper.GetInterface<ICharaTurn>();
        while (turn.IsActing == true)
            await Task.Delay(1);

        m_GameObject.SetActive(true);
        await m_Trap.Effect(m_Setup, stepper, unitFinder, around, m_Effect, m_GameObject.transform.position);
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