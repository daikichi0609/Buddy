using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public interface ICharaBattle : ICharacterComponent
{
    void NormalAttack();
    void NormalAttack(Vector3 direction, InternalDefine.TARGET target);

    void Damage(ICharaBattle oppChara, int power);

    BattleStatus.Parameter Parameter { get; }
    CurrentStatus Status { get; }
}

public class CharaBattle : CharaComponentBase, ICharaBattle
{
    private ICharaMove m_CharaMove;

    private ICharaTurn m_CharaTurn;

    private ICharaAnimator m_CharaAnimator;

    private ICharaCondition m_CharaCondition;

    [SerializeField]
    private Define.CHARA_NAME m_Name = Define.CHARA_NAME.BOXMAN;

    /// <summary>
    /// 元パラメータ
    /// </summary>
    private BattleStatus.Parameter m_Parameter;
    BattleStatus.Parameter ICharaBattle.Parameter => m_Parameter;

    /// <summary>
    /// 現在のステータス
    /// </summary>
    private CurrentStatus m_Status;
    CurrentStatus ICharaBattle.Status => m_Status;

    /// <summary>
    /// 通常攻撃演出時間
    /// </summary>
    private static readonly float ms_NormalAttackTotalTime = 0.6f;

    /// <summary>
    /// 通常攻撃ヒットタイミング
    /// </summary>
    private static readonly float ms_NormalAttackHitTime = 0.4f;

    protected override void Initialize()
    {
        m_CharaMove = Collector.GetComponent<ICharaMove>();
        m_CharaTurn = Collector.GetComponent<ICharaTurn>();
        m_CharaAnimator = Collector.GetComponent<ICharaAnimator>();
        m_CharaCondition = Collector.GetComponent<ICharaCondition>();

        m_Parameter = CharaDataManager.LoadCharaParameter(m_Name);
        m_Status = new CurrentStatus(m_Parameter);

        CharaUiHandler.Interface.InitializeCharacterUi(this.Collector);
    }

    /// <summary>
    /// 通常攻撃 プレイヤー用
    /// </summary>
    void ICharaBattle.NormalAttack() => NormalAttack(m_CharaMove.Direction, InternalDefine.TARGET.ENEMY);

    /// <summary>
    /// 通常攻撃 Ai用
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="target"></param>
    private void NormalAttack(Vector3 direction, InternalDefine.TARGET target)
    {
        m_CharaTurn.StartAction();
        m_CharaAnimator.PlayAnimation(ANIMATION_TYPE.ATTACK);
        StartCoroutine(Coroutine.DelayCoroutine(ms_NormalAttackHitTime, () =>
        {
            SoundManager.Instance.Attack_Sword.Play();
        }));

        Attack(direction, target);
    }

    void ICharaBattle.NormalAttack(Vector3 direction, InternalDefine.TARGET target) => NormalAttack(direction, target);

    /// <summary>
    /// 攻撃 空振り考慮のプレイヤー用
    /// </summary>
    /// <param name="attackPos"></param>
    /// <param name="target"></param>
    private void Attack(Vector3 attackPos, InternalDefine.TARGET target)
    {
        //攻撃音
        StartCoroutine(Coroutine.DelayCoroutine(ms_NormalAttackHitTime, () =>
        {
            SoundManager.Instance.Miss.Play();
        }));

        //アクション終了
        StartCoroutine(Coroutine.DelayCoroutine(ms_NormalAttackTotalTime, () =>
        {
            m_CharaTurn.FinishAction();
        }));

        //攻撃対象がいるか
        if (ConfirmAttack(attackPos, target, false) == false)
            return;

        //ターゲットの情報取得
        if (UnitManager.Interface.TryGetSpecifiedPositionPlayer(attackPos, out var collector) == false)
            return;

        if (collector.Require<ICharaBattle>(out var battle) == false)
            return;

        CurrentStatus targetStatus = battle.Status;

        //ヒットorノット判定
        if (Calculator.JudgeHit(m_Status.Dex, targetStatus.Eva) == false)
        {
            //攻撃音
            StartCoroutine(Coroutine.DelayCoroutine(ms_NormalAttackTotalTime, () =>
            {
                SoundManager.Instance.Miss.Play();
            }));
            return;
        }

        //威力計算
        int power = m_Status.Atk;

        //ダメージはモーション終わりに実行
        StartCoroutine(Coroutine.DelayCoroutine(ms_NormalAttackTotalTime, () =>
        {
            battle.Damage(this, power);
        }));

        m_CharaTurn.FinishTurn();
    }

    private bool ConfirmAttack(Vector3 attackPos, InternalDefine.TARGET target, bool isPossibleToDiagonal)
    {
        //壁抜け不可能ならできなくする
        if (isPossibleToDiagonal == false)
            if (DungeonHandler.Interface.CanMoveDiagonal(m_CharaMove.Position, m_CharaMove.Direction)== false)
                return false;

        //攻撃範囲に攻撃対象がいるか確認
        switch (target)
        {
            case InternalDefine.TARGET.PLAYER:
                return UnitManager.Interface.IsPlayerOn(attackPos);

            case InternalDefine.TARGET.ENEMY:
                return UnitManager.Interface.IsEnemyOn(attackPos);

            case InternalDefine.TARGET.NONE:
                return UnitManager.Interface.IsUnitOn(attackPos);
        }

        return false;
    }

    /// <summary>
    /// スキル
    /// </summary>
    protected virtual void Skill()
    {

    }

    /// <summary>
    /// 被ダメージ
    /// </summary>
    /// <param name="power"></param>
    /// <param name="dex"></param>
    void ICharaBattle.Damage(ICharaBattle oppChara, int power)
    {
        //ダメージ処理
        int damage = Calculator.CalculateDamage(power, m_Parameter.Def);
        m_Status.Hp = Calculator.CalculateRemainingHp(m_Status.Hp, damage);

        SoundManager.Instance.Damage_Small.Play();
        m_CharaAnimator.PlayAnimation(ANIMATION_TYPE.DAMAGE);
        StartCoroutine(Coroutine.DelayCoroutine(0.5f, () =>
        {
            if (m_Parameter.MaxHp <= 0)
            {
                Death();
                MessageBroker.Default.Publish(new Message.MFinishDamage(oppChara, true, true));
            }
            else
            {
                MessageBroker.Default.Publish(new Message.MFinishDamage(oppChara, true, false));
            }
        }));
    }

    protected virtual void Death()
    {
        
    }
}


public class CurrentStatus
{
    /// <summary>
    /// 初期化
    /// </summary>
    /// <param name="param"></param>
    public CurrentStatus(BattleStatus.Parameter param)
    {
        Hp = param.MaxHp;
        Atk = param.Atk;
        Def = param.Def;
        Agi = param.Agi;
        Dex = param.Dex;
        Eva = param.Eva;
        CriticalRate = param.CriticalRate;
        Res = param.Res;
    }

    // レベル
    public int Lv { get; set; } = 1;

    // ヒットポイント
    public int Hp { get; set; }

    // 攻撃力
    public int Atk { get; set; }

    // 防御力
    public int Def { get; set; }

    // 速さ
    public int Agi { get; set; }

    // 命中率補正
    public float Dex { get; set; }

    // 回避率補正
    public float Eva { get; set; }

    // 会心率補正
    public float CriticalRate { get; set; }

    // 抵抗率
    public float Res { get; set; }
}