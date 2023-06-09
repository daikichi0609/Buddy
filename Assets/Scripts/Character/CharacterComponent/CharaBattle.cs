﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System.Threading.Tasks;
using Zenject;

public interface ICharaBattle : IActorInterface
{
    /// <summary>
    /// 通常攻撃
    /// </summary>
    /// <returns></returns>
    bool NormalAttack();

    /// <summary>
    /// 通常攻撃、方向指定
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    bool NormalAttack(DIRECTION direction, CHARA_TYPE target);

    /// <summary>
    /// 被ダメージ
    /// </summary>
    /// <param name="attackInfo"></param>
    /// <returns></returns>
    AttackResult Damage(AttackInfo attackInfo);

    /// <summary>
    /// 割合ダメージ
    /// </summary>
    /// <param name="ratio"></param>
    /// <returns></returns>
    AttackResult DamagePercentage(AttackPercentageInfo attackInfo);
}

public interface ICharaBattleEvent : IActorEvent
{
    /// <summary>
    /// 攻撃前
    /// </summary>
    IObservable<AttackInfo> OnAttackStart { get; }

    /// <summary>
    /// 攻撃後
    /// </summary>
    IObservable<AttackResult> OnAttackEnd { get; }

    /// <summary>
    /// ダメージ前
    /// </summary>
    IObservable<AttackResult> OnDamageStart { get; }

    /// <summary>
    /// ダメージ後
    /// </summary>
    IObservable<AttackResult> OnDamageEnd { get; }

    /// <summary>
    /// 死亡時
    /// </summary>
    IObservable<Unit> OnDead { get; }
}

public class CharaBattle : ActorComponentBase, ICharaBattle, ICharaBattleEvent
{
    [Inject]
    private IDungeonHandler m_DungeonHandler;
    [Inject]
    private ITurnManager m_TurnManager;
    [Inject]
    private IUnitHolder m_UnitHolder;
    [Inject]
    private IUnitFinder m_UnitFinder;
    [Inject]
    private IBattleLogManager m_BattleLogManager;
    [Inject]
    private IAttackResultUiManager m_AttackResultUiManager;

    private ICharaStatus m_CharaStatus;
    private ICharaMove m_CharaMove;
    private ICharaTurn m_CharaTurn;
    private ICharaObjectHolder m_CharaObjectHolder;
    private ICharaLastActionHolder m_CharaLastActionHolder;

    public static readonly int ms_NormalAttackTotalTime = 700;
    public static readonly int ms_NormalAttackHitTime = 400;
    public static readonly int ms_DamageTotalTime = 500;

    private CurrentStatus Status => m_CharaStatus.CurrentStatus;

    /// <summary>
    /// 攻撃前に呼ばれる
    /// </summary>
    private Subject<AttackInfo> m_OnAttackStart = new Subject<AttackInfo>();
    IObservable<AttackInfo> ICharaBattleEvent.OnAttackStart => m_OnAttackStart;

    /// <summary>
    /// 攻撃後に呼ばれる
    /// </summary>
    private Subject<AttackResult> m_OnAttackEnd = new Subject<AttackResult>();
    IObservable<AttackResult> ICharaBattleEvent.OnAttackEnd => m_OnAttackEnd;

    /// <summary>
    /// ダメージ前に呼ばれる
    /// </summary>
    private Subject<AttackResult> m_OnDamageStart = new Subject<AttackResult>();
    IObservable<AttackResult> ICharaBattleEvent.OnDamageStart => m_OnDamageStart;

    /// <summary>
    /// ダメージ後に呼ばれる
    /// </summary>
    private Subject<AttackResult> m_OnDamageEnd = new Subject<AttackResult>();
    IObservable<AttackResult> ICharaBattleEvent.OnDamageEnd => m_OnDamageEnd;

    /// <summary>
    /// 死亡時に呼ばれる
    /// </summary>
    private Subject<Unit> m_OnDead = new Subject<Unit>();
    IObservable<Unit> ICharaBattleEvent.OnDead => m_OnDead;

    protected override void Register(ICollector owner)
    {
        base.Register(owner);
        owner.Register<ICharaBattle>(this);
        owner.Register<ICharaBattleEvent>(this);
    }

    protected override void Initialize()
    {
        base.Initialize();

        m_CharaStatus = Owner.GetInterface<ICharaStatus>();
        m_CharaMove = Owner.GetInterface<ICharaMove>();
        m_CharaTurn = Owner.GetInterface<ICharaTurn>();
        m_CharaObjectHolder = Owner.GetInterface<ICharaObjectHolder>();
        m_CharaLastActionHolder = Owner.GetInterface<ICharaLastActionHolder>();

        // 攻撃時、アクション登録
        m_OnAttackStart.SubscribeWithState(this, (_, self) =>
        {
            self.m_CharaLastActionHolder.RegisterAction(CHARA_ACTION.ATTACK);
        }).AddTo(CompositeDisposable);

        // 攻撃終了時、Ui表示
        m_OnAttackEnd.SubscribeWithState(this, (result, self) =>
        {
            if (result.IsHit == true)
                self.m_AttackResultUiManager.Damage(result);
            else
                self.m_AttackResultUiManager.Miss(result);
        }).AddTo(CompositeDisposable);

        // 死亡時、リストから抜ける
        m_OnDead.SubscribeWithState(this, (_, self) =>
        {
            self.m_UnitHolder.RemoveUnit(self.Owner);
            self.m_TurnManager.RemoveUnit(self.Owner);
        }).AddTo(CompositeDisposable);
    }

    /// <summary>
    /// 通常攻撃・プレイヤー操作
    /// </summary>
    bool ICharaBattle.NormalAttack() => NormalAttack(m_CharaMove.Direction, CHARA_TYPE.ENEMY);

    /// <summary>
    /// 通常攻撃
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="target"></param>
    private bool NormalAttack(DIRECTION direction, CHARA_TYPE target)
    {
        // 誰かが行動中なら攻撃できない
        if (m_TurnManager.NoOneActing == false)
            return false;

        m_CharaMove.Face(direction); // 向く
        var attackInfo = new AttackInfo(Owner, Status.OriginParam.GivenName, m_CharaStatus.CurrentStatus.Atk, 0.95f, 0.05f, false, direction); // 攻撃情報　
        m_OnAttackStart.OnNext(attackInfo); // Event発火

        var attackPos = m_CharaMove.Position + direction.ToV3Int(); // 攻撃地点

        var disposable = m_TurnManager.RequestProhibitAction(Owner); // 行動禁止

        // モーション終わりに
        StartCoroutine(Coroutine.DelayCoroutine(0.7f, () => AttackInternal(attackPos, target, attackInfo, disposable)));
        return true;
    }

    bool ICharaBattle.NormalAttack(DIRECTION direction, CHARA_TYPE target) => NormalAttack(direction, target);

    /// <summary>
    /// 攻撃
    /// </summary>
    /// <param name="attackPos"></param>
    /// <param name="target"></param>
    private AttackResult AttackInternal(Vector3 attackPos, CHARA_TYPE target, AttackInfo attackInfo, IDisposable disposable)
    {
        // 行動禁止解除
        disposable.Dispose();

        // 角抜け確認
        if (m_DungeonHandler.CanMove(m_CharaMove.Position, m_CharaMove.Direction) == false ||
            m_UnitFinder.TryGetSpecifiedPositionUnit(attackPos, out var collector, target) == false ||
            collector.RequireInterface<ICharaBattle>(out var battle) == false)
        {
            return AttackResult.Invalid;
        }

        var result = battle.Damage(attackInfo);

        //モーション終わりに実行
        m_OnAttackEnd.OnNext(result);

        return result;
    }

    /// <summary>
    /// 被ダメージ
    /// </summary>
    /// <param name="power"></param>
    /// <param name="dex"></param>
    AttackResult ICharaBattle.Damage(AttackInfo attackInfo)
    {
        var result = BattleSystem.Damage(attackInfo, Owner);
        DamageInternal(result, attackInfo.Direction);
        return result;
    }

    /// <summary>
    /// 割合ダメージ
    /// </summary>
    /// <param name="ratio"></param>
    /// <returns></returns>
    AttackResult ICharaBattle.DamagePercentage(AttackPercentageInfo attackInfo)
    {
        var result = BattleSystem.DamagePercentage(attackInfo, Owner);
        DamageInternal(result, attackInfo.Direction);
        return result;
    }

    /// <summary>
    /// ダメージ処理
    /// </summary>
    /// <param name="result"></param>
    /// <param name="attackDir"></param>
    private void DamageInternal(AttackResult result, DIRECTION attackDir)
    {
        m_OnDamageStart.OnNext(result);

        if (result.IsHit == false)
            return;

        m_CharaMove.Face(attackDir.ToOppositeDir());

        // awaitしない
        var _ = PostDamage(result);
    }

    /// <summary>
    /// ダメージモーション終わり
    /// </summary>
    /// <param name="result"></param>
    private async Task PostDamage(AttackResult result)
    {
        await Task.Delay(ms_DamageTotalTime); // モーション終わりまで待機
        m_OnDamageEnd.OnNext(result);

        // 死亡
        if (result.IsDead == true)
            Dead();
    }

    /// <summary>
    /// 死亡時
    /// </summary>
    private void Dead()
    {
        m_OnDead.OnNext(Unit.Default);
        Owner.Dispose();
    }
}