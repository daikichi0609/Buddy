using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System.Threading.Tasks;

public interface ICharaBattle : ICharacterInterface
{
    Task NormalAttack();
    Task NormalAttack(DIRECTION direction, CHARA_TYPE target);

    AttackResult Damage(AttackInfo attackInfo);
}

public interface ICharaBattleEvent : ICharacterEvent
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

public class CharaBattle : CharaComponentBase, ICharaBattle, ICharaBattleEvent
{
    private ICharaStatus m_CharaStatus;
    private ICharaMove m_CharaMove;
    private ICharaTurn m_CharaTurn;
    private ICharaObjectHolder m_CharaObjectHolder;

    public static readonly int ms_NormalAttackTotalTime = 700;
    public static readonly int ms_NormalAttackHitTime = 400;
    public static readonly int ms_DamageTotalTime = 500;

    /// <summary>
    /// 味方か敵か
    /// </summary>
    private CHARA_TYPE m_Type = CHARA_TYPE.NONE;

    /// <summary>
    /// ステータス
    /// </summary>
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

        if (Owner.RequireInterface<IEnemyAi>(out var enemy) == true)
            m_Type = CHARA_TYPE.ENEMY;
        else
            m_Type = CHARA_TYPE.PLAYER;
    }

    /// <summary>
    /// 通常攻撃・プレイヤー操作
    /// </summary>
    async Task ICharaBattle.NormalAttack() => await NormalAttack(m_CharaMove.Direction, CHARA_TYPE.ENEMY);

    /// <summary>
    /// 通常攻撃
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="target"></param>
    private async Task NormalAttack(DIRECTION direction, CHARA_TYPE target)
    {
        // 誰かが行動中なら攻撃できない
        if (TurnManager.Interface.NoOneActing == false)
            return;

        var attackInfo = new AttackInfo(Owner, Status.Name, Status.Atk, Status.Dex);
        m_OnAttackStart.OnNext(attackInfo);

        var attackPos = m_CharaMove.Position + direction.ToV3Int();
        var result = await AttackInternal(attackPos, target, attackInfo);

        //モーション終わりに実行
        m_OnAttackEnd.OnNext(result);

        m_CharaTurn.TurnEnd();
    }

    async Task ICharaBattle.NormalAttack(DIRECTION direction, CHARA_TYPE target) => await NormalAttack(direction, target);

    /// <summary>
    /// 攻撃 空振り考慮のプレイヤー用
    /// </summary>
    /// <param name="attackPos"></param>
    /// <param name="target"></param>
    private async Task<AttackResult> AttackInternal(Vector3 attackPos, CHARA_TYPE target, AttackInfo attackInfo)
    {
        await Task.Delay(ms_NormalAttackTotalTime); // モーション終わりまで

        // 角抜け確認
        if (DungeonHandler.Interface.CanMove(m_CharaMove.Position, m_CharaMove.Direction) == false)
            return AttackResult.Invalid;

        // ターゲットの情報取得
        if (UnitFinder.Interface.TryGetSpecifiedPositionUnit(attackPos, out var collector, target) == false)
            return AttackResult.Invalid;

        // 必要なコンポーネント
        if (collector.RequireInterface<ICharaBattle>(out var battle) == false)
            return AttackResult.Invalid;

        return battle.Damage(attackInfo);
    }

    /// <summary>
    /// 被ダメージ
    /// </summary>
    /// <param name="power"></param>
    /// <param name="dex"></param>
    AttackResult ICharaBattle.Damage(AttackInfo attackInfo)
    {
        var isHit = Calculator.JudgeHit(attackInfo.Dex, Status.Eva);

        //ダメージ処理
        int damage = Calculator.CalculateDamage(attackInfo.Atk, Status.Def);
        Status.Hp = Calculator.CalculateRemainingHp(Status.Hp, damage);
        bool isDead = Status.Hp == 0;

        var result = new AttackResult(attackInfo, Owner, Status.Name, isHit, damage, Status.Hp, isDead);

        if (isHit == false)
            return result;

        m_OnDamageStart.OnNext(result);

        // awaitしない
        PostDamage(result);

        return result;
    }

    /// <summary>
    /// ダメージモーション終わり
    /// </summary>
    /// <param name="result"></param>
    private async void PostDamage(AttackResult result)
    {
        await Task.Delay(ms_DamageTotalTime); // モーション終わりまで待機
        m_OnDamageEnd.OnNext(result);
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
        UnitHolder.Interface.RemoveUnit(Owner);
        ObjectPool.Instance.SetObject(m_CharaStatus.CurrentStatus.Name.ToString(), m_CharaObjectHolder.MoveObject);
    }
}