using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public abstract class CharaBattle : MonoBehaviour
{
    /// <summary>
    /// 同じオブジェクトのCharaMove
    /// </summary>
    private CharaMove m_CharaMove;
    protected CharaMove CharaMove
    {
        get => m_CharaMove;
        set => m_CharaMove = value;
    }

    private CharaTurn m_CharaTurn;
    protected CharaTurn CharaTurn
    {
        get => m_CharaTurn;
        set => m_CharaTurn = value;
    }

    /// <summary>
    /// パラメータ
    /// </summary>
    [SerializeField] private BattleStatus.Parameter m_Parameter;
    public BattleStatus.Parameter Parameter
    {
        get { return m_Parameter; }
        set { m_Parameter = value; }
    }

    /// <summary>
    /// 状態異常
    /// </summary>
    protected CharaCondition m_Condition;
    protected CharaCondition Condition
    {
        get { return m_Condition; }
        set { m_Condition = value; }
    }

    /// <summary>
    /// レベル
    /// </summary>
    public int Lv
    {
        private get; set;
    }

    /// <summary>
    /// HP最大値
    /// </summary>
    public int MaxHp
    {
        get;
        private set;
    }

    public virtual void Initialize()
    {
        CharaMove = GetComponent<CharaMove>();
        CharaTurn = GetComponent<CharaTurn>();
        Condition = GetComponent<CharaCondition>();
        MaxHp = Parameter.Hp;

        //ダメージ処理が終わったら制限解除
        MessageBroker.Default.Receive<Message.MFinishDamage>().Subscribe(_ =>
        {
            if(_.Chara == this)
            {
                CharaTurn.CAN_ACTION = true;
                CharaTurn.FinishTurn();
            }
        });
    }

    /// <summary>
    /// 通常攻撃情報
    /// </summary>
    protected virtual AttackInfo AttackInfo { get; }

    /// <summary>
    /// 通常攻撃 共通部分
    /// </summary>
    protected virtual void NormalAttack()
    {
        //他キャラの行動禁止
        CharaTurn.CAN_ACTION = false;

        //モーション終わりにターンを返す
        SwitchIsAttacking(AttackInfo.ActFrame);

        //アニメーション再生
        PlayAnimation("IsAttacking");
    }

    /// <summary>
    /// 攻撃 空振り考慮のプレイヤー用
    /// </summary>
    /// <param name="attackPos"></param>
    /// <param name="target"></param>
    protected virtual void Attack(Vector3 attackPos, InternalDefine.TARGET target)
    {
        //攻撃対象がいるか
        bool isSuccess = ConfirmAttack(attackPos, target);
        if(isSuccess == false)
        {
            StartCoroutine(Coroutine.DelayCoroutine(AttackInfo.AnimFrame, () =>
            {
               PlaySound(false, null);
               CharaTurn.CAN_ACTION = true;
               CharaTurn.FinishTurn();
            }));
            return;
        }

        //ターゲットの情報取得
        CharaBattle targetBattle = ObjectManager.Instance.SpecifiedPositionCharacterObject(attackPos).GetComponent<CharaBattle>();
        BattleStatus.Parameter targetParam = targetBattle.Parameter;

        //威力計算
        float mag = Calculator.CalculateNormalAttackMag(AttackInfo.Lv, AttackInfo.Mag);
        int power = Calculator.CalculatePower(Parameter.Atk, mag);

        //音再生は剣のヒット時
        StartCoroutine(Coroutine.DelayCoroutine(AttackInfo.AnimFrame, () =>
        {
            PlaySound(true, SoundManager.Instance.Attack_Sword);
        }));

        //ダメージはモーション終わりに実行
        StartCoroutine(Coroutine.DelayCoroutine(AttackInfo.AnimFrame, () =>
        {
            targetBattle.Damage(this, power, Parameter.Dex);
        }));
    }

    private bool ConfirmAttack(Vector3 attackPos, InternalDefine.TARGET target)
    {
        //壁抜け不可能ならできなくする
        if (AttackInfo.IsPossibleToDiagonal == false)
        {
            if (DungeonTerrain.Instance.IsPossibleToMoveDiagonal((int)CharaMove.Position.x, (int)CharaMove.Position.z, (int)CharaMove.Direction.x, (int)CharaMove.Direction.z) == false)
            {
                return false;
            }
        }

        //攻撃範囲に攻撃対象がいるか確認
        switch (target)
        {
            case InternalDefine.TARGET.PLAYER:
                if (Positional.IsPlayerOn(attackPos) == false)
                {
                    return false;
                }
                break;

            case InternalDefine.TARGET.ENEMY:
                if (Positional.IsEnemyOn(attackPos) == false)
                {
                    return false;
                }
                break;

            case InternalDefine.TARGET.NONE:
                if (Positional.IsCharacterOn(attackPos) == false)
                {
                    return false;
                }
                break;
        }

        return true;
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
    public void Damage(CharaBattle opponentChara, int power, float dex)
    {
        //ヒットorノット判定
        if (Calculator.JudgeHit(dex, Parameter.Eva) == false)
        {
            PlaySound(false, null);
            MessageBroker.Default.Publish(new Message.MFinishDamage(opponentChara, false, true));
            return;
        }

        //ダメージ処理
        int damage = Calculator.CalculateDamage(power, Parameter.Def);
        Parameter.Hp = Calculator.CalculateRemainingHp(Parameter.Hp, damage);

        SoundManager.Instance.Damage_Small.Play();
        PlayAnimation("IsDamaging");
        StartCoroutine(Coroutine.DelayCoroutine(0.5f, () =>
        {
            if (Parameter.Hp <= 0)
            {
                Death();
                MessageBroker.Default.Publish(new Message.MFinishDamage(opponentChara, true, true));
            }
            else
            {
                MessageBroker.Default.Publish(new Message.MFinishDamage(opponentChara, true, false));
            }
        }));
    }

    protected virtual void Death()
    {
        
    }

    /// <summary>
    /// アニメーションを一回流す
    /// </summary>
    /// <param name="name"></param>
    protected void PlayAnimation(string name)
    {
        CharaMove.CharaAnimator.SetBool(name, true);
        StartCoroutine(Coroutine.DelayCoroutine(0.1f, () =>
        {
            CharaMove.CharaAnimator.SetBool(name, false);
        }));
    }

    /// <summary>
    /// 攻撃の全体フレームに合わせてAction中にする
    /// </summary>
    /// <param name="actFrame"></param>
    protected void SwitchIsAttacking(float actFrame)
    {
        CharaTurn.StartAction();
        
        StartCoroutine(Coroutine.DelayCoroutine(actFrame, () =>
        {
            CharaTurn.FinishAction();
        }));
    }

    /// <summary>
    /// ヒット音を鳴らす
    /// </summary>
    /// <param name="hit"></param>
    /// <param name="delayFrame"></param>
    /// <param name="sound"></param>
    protected void PlaySound(bool hit, AudioSource sound) //攻撃サウンド
    {
        if (hit == false)
        {
            SoundManager.Instance.Miss.Play();
        }
        else if (hit == true)
        {
             sound.Play();
        }
    }
}

/// <summary>
/// 通常攻撃基底クラス
/// </summary>
public abstract class AttackInfo
{
    public virtual string Name { get; } //技名
    public int Lv { get; set; } = 1; //技レベル

    public virtual float Mag { get; } //攻撃倍率
    public virtual float ActFrame { get; } //モーション全体フレーム
    public virtual float AnimFrame { get; } //アニメーション秒数
    public virtual bool IsPossibleToDiagonal { get; } //斜め壁抜け可能かどうか
}