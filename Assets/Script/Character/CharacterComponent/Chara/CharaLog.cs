using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System.Text;

public interface ICharaLog : ICharacterInterface
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

        if (Owner.RequireEvent<ICharaBattleEvent>(out var battle) == true)
        {
            // 攻撃ログ
            battle.OnAttackStart.Subscribe(info =>
            {
                var log = CreateAttackLog(info);
                BattleLogManager.Interface.Log(log);
            }).AddTo(Disposable);

            // 攻撃結果ログ
            battle.OnAttackEnd.Subscribe(result =>
            {
                var log = CreateAttackResultLog(result);
                BattleLogManager.Interface.Log(log);
            }).AddTo(Disposable);

            // 死亡ログ
            battle.OnDamageEnd.Subscribe(result =>
            {
                if (result.IsDead == false)
                    return;

                var log = CreateDeadLog(result);
                BattleLogManager.Interface.Log(log);
            }).AddTo(Disposable);
        }

        if (Owner.RequireInterface<ICharaInventoryEvent>(out var inventory) == true)
        {
            inventory.OnPutItem.Subscribe(info =>
            {
                if (info.Owner.RequireInterface<ICharaStatus>(out var status) == false)
                    return;

                var log = CreatePutItemLog(status.CurrentStatus.Name, info.Item);
                BattleLogManager.Interface.Log(log);
            }).AddTo(Disposable);
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
        string attacker = info.Name.ToString();

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

        string defender = result.Name.ToString();
        string damage = result.Damage.ToString();

        if (result.IsHit == false)
        {
            if (result.Name != CHARA_NAME.NONE)
                sb.Append("しかし" + defender + "には当たらなかった");

            return sb.ToString();
        }

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
        string defender = result.Name.ToString();

        sb.Append(defender + "は倒れた！");

        return sb.ToString();
    }

    /// <summary>
    /// 死亡ログ
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    private string CreatePutItemLog(CHARA_NAME name, IItem item)
    {
        var sb = new StringBuilder();

        sb.Append(name + "は" + item.Name + "を拾った");

        return sb.ToString();
    }
}
