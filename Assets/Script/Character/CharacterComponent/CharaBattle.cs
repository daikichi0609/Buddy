using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System.Threading.Tasks;

public interface ICharaBattle : ICharacterComponent
{
    Task NormalAttack();
    Task NormalAttack(Vector3 direction, InternalDefine.CHARA_TYPE target);

    AttackResult Damage(AttackInfo attackInfo);
}

public interface ICharaBattleEvent : ICharacterComponent
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
}

public class CharaBattle : CharaComponentBase, ICharaBattle, ICharaBattleEvent
{
    private ICharaStatus m_CharaStatus;
    private ICharaMove m_CharaMove;
    private ICharaTurn m_CharaTurn;

    public static readonly int ms_NormalAttackTotalTime = 700;
    public static readonly int ms_NormalAttackHitTime = 400;
    public static readonly int ms_DamageTotalTime = 500;

    /// <summary>
    /// 味方か敵か
    /// </summary>
    private InternalDefine.CHARA_TYPE m_Type = InternalDefine.CHARA_TYPE.NONE;

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

    protected override void Register(ICollector owner)
    {
        base.Register(owner);
        owner.Register<ICharaBattle>(this);
        owner.Register<ICharaBattleEvent>(this);
    }

    protected override void Initialize()
    {
        m_CharaStatus = Owner.GetComponent<ICharaStatus>();
        m_CharaMove = Owner.GetComponent<ICharaMove>();
        m_CharaTurn = Owner.GetComponent<ICharaTurn>();

        if (Owner.RequireComponent<IEnemyAi>(out var enemy) == true)
            m_Type = InternalDefine.CHARA_TYPE.ENEMY;
        else
            m_Type = InternalDefine.CHARA_TYPE.PLAYER;
    }

    /// <summary>
    /// 通常攻撃・プレイヤー操作
    /// </summary>
    async Task ICharaBattle.NormalAttack() => await NormalAttack(m_CharaMove.Direction, InternalDefine.CHARA_TYPE.ENEMY);

    /// <summary>
    /// 通常攻撃
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="target"></param>
    private async Task NormalAttack(Vector3 direction, InternalDefine.CHARA_TYPE target)
    {
        // 誰かが行動中なら攻撃できない
        if (TurnManager.Interface.NoOneActing == false)
            return;

        var attackInfo = new AttackInfo(Status.Name, Status.Atk, Status.Dex);
        m_OnAttackStart.OnNext(attackInfo);

        var attackPos = m_CharaMove.Position + direction;
        var result = await AttackInternal(attackPos, target, attackInfo);

        //モーション終わりに実行
        m_OnAttackEnd.OnNext(result);
    }

    async Task ICharaBattle.NormalAttack(Vector3 direction, InternalDefine.CHARA_TYPE target) => await NormalAttack(direction, target);

    /// <summary>
    /// 攻撃 空振り考慮のプレイヤー用
    /// </summary>
    /// <param name="attackPos"></param>
    /// <param name="target"></param>
    private async Task<AttackResult> AttackInternal(Vector3 attackPos, InternalDefine.CHARA_TYPE target, AttackInfo attackInfo)
    {
        await Task.Delay(ms_NormalAttackTotalTime); // モーション終わりまで
        Debug.Log("Attack End");

        // 角抜け確認
        if (DungeonHandler.Interface.CanMoveDiagonal(m_CharaMove.Position, m_CharaMove.Direction) == false)
            return AttackResult.Invalid;

        // ターゲットの情報取得
        if (UnitManager.Interface.TryGetSpecifiedPositionUnit(attackPos, out var collector, target) == false)
            return AttackResult.Invalid;

        // 必要なコンポーネント
        if (collector.RequireComponent<ICharaBattle>(out var battle) == false)
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

        if (isHit == false)
            return AttackResult.Invalid;

        //ダメージ処理
        int damage = Calculator.CalculateDamage(attackInfo.Atk, Status.Def);
        Status.Hp = Calculator.CalculateRemainingHp(Status.Hp , damage);
        bool isDead = Status.Hp == 0;

        var result = new AttackResult(attackInfo, Status.Name, isHit, damage, Status.Hp, isDead);
        m_OnDamageStart.OnNext(result);

        // awaitしない
        var _ = Task.Run(async () =>
        {
            await Task.Delay(ms_DamageTotalTime); // モーション終わりまで待機
            m_OnDamageEnd.OnNext(result);
            if (isDead == true)
                Death();
        });

        return result;
    }

    protected virtual void Death()
    {

    }
}