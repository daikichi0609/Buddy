using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System.Text;

public interface ICharaLog : ICharacterComponent
{

}

public class CharaLog : CharaComponentBase, ICharaLog
{
    protected override void Register(ICollector owner)
    {
        base.Register(owner);
        owner.Register<ICharaLog>(this);
    }

    protected override void Initialize()
    {
        base.Initialize();

        if (Owner.RequireComponent<ICharaBattleEvent>(out var battle) == true)
        {
            // 攻撃ログ
            battle.OnAttackStart.Subscribe(info =>
            {
                var log = CreateAttackLog(info);
                BattleLogManager.Interface.Log(log);
            });

            // 攻撃結果ログ
            battle.OnDamageStart.Subscribe(result =>
            {
                // ヒットしていないならログを出さない
                if (result.IsHit == false)
                    return;

                var log = CreateAttackResultLog(result);
                BattleLogManager.Interface.Log(log);
            }).AddTo(this);

            // 死亡ログ
            battle.OnDamageEnd.Subscribe(result =>
            {
                if (result.IsDead == false)
                    return;

                var log = CreateDeadLog(result);
                BattleLogManager.Interface.Log(log);
            }).AddTo(this);
        }

    }

    /// <summary>
    /// 攻撃ログ作成
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    private string CreateAttackLog(AttackInfo info)
    {
        var sb = new StringBuilder();
        string attacker = info.Attacker.ToString();

        sb.Append(attacker + "の攻撃！");

        return sb.ToString();
    }

    /// <summary>
    /// 攻撃結果ログ作成
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    private string CreateAttackResultLog(AttackResult result)
    {
        var sb = new StringBuilder();
        string defender = result.Defender.ToString();
        string damage = result.Damage.ToString();

        sb.Append(defender + "に" + damage + "ダメージ");

        return sb.ToString();
    }

    /// <summary>
    /// 死亡ログ
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    private string CreateDeadLog(AttackResult result)
    {
        var sb = new StringBuilder();
        string defender = result.Defender.ToString();

        sb.Append(defender + "は倒れた！");

        return sb.ToString();
    }
}
