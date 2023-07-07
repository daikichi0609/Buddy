using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System.Threading.Tasks;
using Zenject;
using static UnityEngine.UI.GridLayoutGroup;

public interface ICharaBattle : IActorInterface
{
    /// <summary>
    /// 通常攻撃
    /// </summary>
    /// <returns></returns>
    Task<bool> NormalAttack();

    /// <summary>
    /// 通常攻撃、方向指定
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    Task<bool> NormalAttack(DIRECTION direction, CHARA_TYPE target);

    /// <summary>
    /// 被ダメージ
    /// </summary>
    /// <param name="attackInfo"></param>
    /// <returns></returns>
    Task<AttackResult> Damage(AttackInfo attackInfo);

    /// <summary>
    /// 割合ダメージ
    /// </summary>
    /// <param name="ratio"></param>
    /// <returns></returns>
    Task<AttackResult> DamagePercentage(AttackPercentageInfo attackInfo);
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
    IObservable<AttackResult> OnDead { get; }
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
    private IAttackResultUiManager m_AttackResultUiManager;

    private ICharaStatus m_CharaStatus;
    private ICharaMove m_CharaMove;
    private ICharaLastActionHolder m_CharaLastActionHolder;
    private CurrentStatus Status => m_CharaStatus.CurrentStatus;
    private ICharaTurn m_CharaTurn;
    private ICharaAnimator m_CharaAnimator;

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
    private Subject<AttackResult> m_OnDead = new Subject<AttackResult>();
    IObservable<AttackResult> ICharaBattleEvent.OnDead => m_OnDead;

    public static readonly float ms_NormalAttackTotalTime = 0.7f;
    public static readonly float ms_NormalAttackHitTime = 0.4f;
    public static readonly float ms_DamageTotalTime = 0.5f;

    public static readonly float HIT_PROB = 0.95f;
    public static readonly float CRITICAL_PROB = 0.05f;

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
        m_CharaLastActionHolder = Owner.GetInterface<ICharaLastActionHolder>();
        m_CharaTurn = Owner.GetInterface<ICharaTurn>();
        m_CharaAnimator = Owner.GetInterface<ICharaAnimator>();

        // 攻撃時、アクション登録
        m_OnAttackStart.SubscribeWithState(this, (_, self) =>
        {
            self.m_CharaLastActionHolder.RegisterAction(CHARA_ACTION.ATTACK);
        }).AddTo(Owner.Disposables);

        // 攻撃終了時、Ui表示
        m_OnDamageStart.SubscribeWithState(this, (result, self) =>
        {
            if (result.IsHit == true)
                self.m_AttackResultUiManager.Damage(result);
            else
                self.m_AttackResultUiManager.Miss(result);
        }).AddTo(Owner.Disposables);

        // 死亡時、リストから抜ける
        m_OnDead.SubscribeWithState(this, (_, self) =>
        {
            self.m_UnitHolder.RemoveUnit(self.Owner);
            self.m_TurnManager.RemoveUnit(self.Owner);
        }).AddTo(Owner.Disposables);
    }

    /// <summary>
    /// 通常攻撃・プレイヤー操作
    /// </summary>
    async Task<bool> ICharaBattle.NormalAttack() => await NormalAttack(m_CharaMove.Direction, CHARA_TYPE.ENEMY);

    /// <summary>
    /// 通常攻撃
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="target"></param>
    private async Task<bool> NormalAttack(DIRECTION direction, CHARA_TYPE target)
    {
        // 誰かが行動中なら攻撃できない
        if (m_TurnManager.NoOneActing == false)
            return false;

        await m_CharaMove.Face(direction); // 向く
        var attackInfo = new AttackInfo(Owner, Status.OriginParam.GivenName, m_CharaStatus.CurrentStatus.Atk, HIT_PROB, CRITICAL_PROB, false, direction); // 攻撃情報　
        m_OnAttackStart.OnNext(attackInfo); // Event発火

        var attackPos = m_CharaMove.Position + direction.ToV3Int(); // 攻撃地点
        await AttackInternal(attackPos, target, attackInfo);
        return true;
    }

    Task<bool> ICharaBattle.NormalAttack(DIRECTION direction, CHARA_TYPE target) => NormalAttack(direction, target);

    /// <summary>
    /// 攻撃
    /// </summary>
    /// <param name="attackPos"></param>
    /// <param name="target"></param>
    private async Task<AttackResult> AttackInternal(Vector3 attackPos, CHARA_TYPE target, AttackInfo attackInfo)
    {
        await m_CharaAnimator.PlayAnimation(ANIMATION_TYPE.ATTACK, ms_NormalAttackTotalTime);

        // 角抜け確認
        if (m_DungeonHandler.CanMove(m_CharaMove.Position, m_CharaMove.Direction) == false ||
            m_UnitFinder.TryGetSpecifiedPositionUnit(attackPos, out var collector, target) == false ||
            collector.RequireInterface<ICharaBattle>(out var battle) == false)
        {
            return AttackResult.Invalid;
        }

        var result = await battle.Damage(attackInfo);

        // モーション終わりに実行
        m_OnAttackEnd.OnNext(result);

        return result;
    }

    /// <summary>
    /// 被ダメージ
    /// </summary>
    /// <param name="power"></param>
    /// <param name="dex"></param>
    async Task<AttackResult> ICharaBattle.Damage(AttackInfo attackInfo)
    {
        var result = BattleSystem.Damage(attackInfo, Owner);
        await Damage(result, attackInfo.Direction);
        return result;
    }

    /// <summary>
    /// 割合ダメージ
    /// </summary>
    /// <param name="ratio"></param>
    /// <returns></returns>
    async Task<AttackResult> ICharaBattle.DamagePercentage(AttackPercentageInfo attackInfo)
    {
        var result = BattleSystem.DamagePercentage(attackInfo, Owner);
        await Damage(result, attackInfo.Direction);
        return result;
    }

    /// <summary>
    /// ダメージ処理
    /// </summary>
    /// <param name="result"></param>
    /// <param name="attackDir"></param>
    private async Task Damage(AttackResult result, DIRECTION attackDir)
    {
        m_OnDamageStart.OnNext(result);

        if (result.IsHit == false)
            return;

        await m_CharaMove.Face(attackDir.ToOppositeDir());

        await DamageInternal(result);
    }

    /// <summary>
    /// ダメージモーション終わり
    /// </summary>
    /// <param name="result"></param>
    private async Task DamageInternal(AttackResult result)
    {
        await m_CharaAnimator.PlayAnimation(ANIMATION_TYPE.DAMAGE, ms_DamageTotalTime);
        m_OnDamageEnd.OnNext(result);

        // 死亡
        if (result.IsDead == true)
            Dead(result);
    }

    /// <summary>
    /// 死亡時
    /// </summary>
    private void Dead(AttackResult result)
    {
        m_OnDead.OnNext(result);
        Owner.Dispose();
    }
}