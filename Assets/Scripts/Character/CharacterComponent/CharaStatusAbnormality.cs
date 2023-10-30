using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;
using Zenject;
using static UnityEngine.UI.GridLayoutGroup;

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
    /// 喪失状態
    /// </summary>
    bool IsLostOne { get; set; }

    /// <summary>
    /// 食事効果
    /// </summary>
    bool IsAttackUpFood { get; set; }
    bool IsCriticalRatioUpFood { get; set; }

    /// <summary>
    /// 逆上アビリティフラグ
    /// </summary>
    bool CanFrenzy { get; set; }

    /// <summary>
    /// 眠り状態
    /// </summary>
    Task<bool> Sleep();

    /// <summary>
    /// 喪失状態
    /// </summary>
    /// <returns></returns>
    Task<bool> LostOne();
}

public class CharaStatusAbnormality : ActorComponentBase, ICharaStatusAbnormality
{
    [Inject]
    private IBattleLogManager m_BattleLogManager;
    [Inject]
    private IEffectHolder m_EffectHolder;
    [Inject]
    private ISoundHolder m_SoundHolder;

    private ICharaStatus m_CharaStatus;
    private ICharaLastActionHolder m_LastAction;

    /// <summary>
    /// 毒状態
    /// </summary>
    private ReactiveProperty<bool> m_IsPoison = new ReactiveProperty<bool>();
    bool ICharaStatusAbnormality.IsPoison { get => m_IsPoison.Value; set => m_IsPoison.Value = value; }

    /// <summary>
    /// 眠り状態
    /// </summary>
    private ReactiveProperty<bool> m_IsSleeping = new ReactiveProperty<bool>();
    bool ICharaStatusAbnormality.IsSleeping { get => m_IsSleeping.Value; set => m_IsSleeping.Value = value; }

    /// <summary>
    /// 喪失状態
    /// </summary>
    private ReactiveProperty<bool> m_IsLostOne = new ReactiveProperty<bool>();
    bool ICharaStatusAbnormality.IsLostOne { get => m_IsLostOne.Value; set => m_IsLostOne.Value = value; }

    /// <summary>
    /// 逆上アビリティフラグ
    /// </summary>
    private bool m_CanFrenzy;
    bool ICharaStatusAbnormality.CanFrenzy { get => m_CanFrenzy; set => m_CanFrenzy = value; }
    private CompositeDisposable m_FinishFrenzy;

    private static readonly float FRENZY_RATIO = 1.0f;
    private static readonly string FRENZY = "Frenzy";

    /// <summary>
    /// 食事効果
    /// </summary>
    bool ICharaStatusAbnormality.IsAttackUpFood { get; set; }
    bool ICharaStatusAbnormality.IsCriticalRatioUpFood { get; set; }

    protected override void Register(ICollector owner)
    {
        base.Register(owner);
        owner.Register(this);
    }

    protected override void Initialize()
    {
        base.Initialize();
        m_CharaStatus = Owner.GetInterface<ICharaStatus>();
        m_LastAction = Owner.GetInterface<ICharaLastActionHolder>();

        m_IsPoison.SubscribeWithState(this, (isPoison, self) =>
        {
            var status = self.Owner.GetInterface<ICharaStatus>().CurrentStatus;
            var log = self.Owner.GetInterface<ICharaLog>();

            if (self.m_CanFrenzy == false || isPoison == false)
            {
                if (self.m_FinishFrenzy != null)
                {
                    self.m_FinishFrenzy.Dispose();
                    self.m_FinishFrenzy = null;

                    string messege = status.OriginParam.GivenName + "の逆上は収まった。";
                    log.Log(messege);
                }
                return;
            }

            if (self.m_FinishFrenzy == null)
            {
                self.m_FinishFrenzy = new CompositeDisposable();
                // 音
                if (self.m_SoundHolder.TryGetSound(KeyName.BUFF, out var sound) == true)
                    sound.Play();
                // エフェクト
                if (self.m_EffectHolder.TryGetEffect(FRENZY, out var effect) == true)
                    self.m_FinishFrenzy.Add(effect.PlayFollow(self.Owner));
                // バフ
                self.m_FinishFrenzy.Add(status.AddBuff(new BuffTicket(PARAMETER_TYPE.ATK, FRENZY_RATIO)));

                string messege = status.OriginParam.GivenName + "は毒の苦しみに逆上して攻撃力が上がった！";
                log.Log(messege);
            }
        }).AddTo(Owner.Disposables);
    }

    protected override void Dispose()
    {
        m_IsPoison.Value = false;
        m_IsSleeping.Value = false;
        m_IsLostOne.Value = false;
        base.Dispose();
    }

    /// <summary>
    /// 眠り状態
    /// </summary>
    /// <returns></returns>
    async Task<bool> ICharaStatusAbnormality.Sleep()
    {
        if (m_IsSleeping.Value == false)
            return false;

        string log = m_CharaStatus.CurrentStatus.OriginParam.GivenName + "は眠っている";
        m_BattleLogManager.Log(log);

        m_LastAction.RegisterAction(CHARA_ACTION.WAIT);
        await Task.Delay(500);
        return true;
    }

    /// <summary>
    /// 喪失状態
    /// </summary>
    /// <returns></returns>
    async Task<bool> ICharaStatusAbnormality.LostOne()
    {
        if (m_IsLostOne.Value == false)
            return false;

        string log = m_CharaStatus.CurrentStatus.OriginParam.GivenName + "は自我を見失っている！";
        m_BattleLogManager.Log(log);

        m_LastAction.RegisterAction(CHARA_ACTION.WAIT);
        await Task.Delay(500);
        return true;
    }
}
