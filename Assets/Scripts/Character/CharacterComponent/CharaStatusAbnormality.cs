using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public interface ICharaStatusAbnormality : IActorInterface
{
    /// <summary>
    /// 毒状態
    /// </summary>
    bool IsPoison { get; set; }

    /// <summary>
    /// 眠り状態
    /// </summary>
    bool IsSleeping { get; set; }

    /// <summary>
    /// 眠り状態
    /// </summary>
    Task<bool> Sleep();
}

public class CharaStatusAbnormality : ActorComponentBase, ICharaStatusAbnormality
{
    [Inject]
    private IBattleLogManager m_BattleLogManager;

    private ICharaStatus m_CharaStatus;
    private ICharaLastActionHolder m_LastAction;

    protected override void Register(ICollector owner)
    {
        base.Register(owner);
        owner.Register(this);
    }

    protected override void Dispose()
    {
        m_IsPoison = false;
        m_IsSleeping = false;
        base.Dispose();
    }

    /// <summary>
    /// 毒状態
    /// </summary>
    private bool m_IsPoison;
    bool ICharaStatusAbnormality.IsPoison { get => m_IsPoison; set => m_IsPoison = value; }

    /// <summary>
    /// 眠り状態
    /// </summary>
    private bool m_IsSleeping;
    bool ICharaStatusAbnormality.IsSleeping { get => m_IsSleeping; set => m_IsSleeping = value; }

    /// <summary>
    /// 眠り状態
    /// </summary>
    /// <returns></returns>
    async Task<bool> ICharaStatusAbnormality.Sleep()
    {
        if (m_IsSleeping == false)
            return false;

        string log = m_CharaStatus.CurrentStatus.OriginParam.GivenName + "は眠っている";
        m_BattleLogManager.Log(log);

        m_LastAction.RegisterAction(CHARA_ACTION.WAIT);
        await Task.Delay(500);
        return true;
    }
}
