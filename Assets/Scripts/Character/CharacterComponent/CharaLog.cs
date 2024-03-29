using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System.Text;
using Zenject;

public interface ICharaLog : IActorInterface
{
    /// <summary>
    /// BattleLogManagerのLog()呼び出し
    /// </summary>
    /// <param name="messege"></param>
    void Log(string messege);
}

public class CharaLog : ActorComponentBase, ICharaLog
{
    [Inject]
    private IBattleLogManager m_BattleLogManager;

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
            battle.OnAttackStart.SubscribeWithState(this, (info, self) =>
            {
                var log = self.CreateAttackLog(info);
                self.m_BattleLogManager.Log(log);
            }).AddTo(Owner.Disposables);

            // 攻撃結果ログ
            battle.OnDamageStart.SubscribeWithState(this, (result, self) =>
            {
                var log = CreateAttackResultLog(result);
                self.m_BattleLogManager.Log(log);
            }).AddTo(Owner.Disposables);

            // 死亡ログ
            battle.OnDamageEnd.SubscribeWithState(this, (result, self) =>
            {
                if (result.IsDead == false)
                    return;

                var log = self.CreateDeadLog(result);
                self.m_BattleLogManager.Log(log);
            }).AddTo(Owner.Disposables);
        }

        if (Owner.RequireInterface<ICharaInventoryEvent>(out var inventory) == true)
        {
            inventory.OnPutItem.SubscribeWithState(this, (info, self) =>
            {
                if (info.Owner.RequireInterface<ICharaStatus>(out var status) == false)
                    return;

                var log = self.CreatePutItemLog(status.CurrentStatus.OriginParam.GivenName, info.Item);
                self.m_BattleLogManager.Log(log);
            }).AddTo(Owner.Disposables);

            inventory.OnPutItemFail.SubscribeWithState(this, (info, self) =>
            {
                if (info.Owner.RequireInterface<ICharaStatus>(out var status) == false)
                    return;

                var log = self.CreatePutItemFailLog(status.CurrentStatus.OriginParam.GivenName, info.Item);
                self.m_BattleLogManager.Log(log);
            }).AddTo(Owner.Disposables);
        }
    }

    void ICharaLog.Log(string messege) => m_BattleLogManager.Log(messege);

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
    private static string CreateAttackResultLog(AttackResult result)
    {
        var sb = new StringBuilder();

        string defender = result.Name;
        string damage = result.Damage.ToString();

        if (result.IsHit == false)
        {
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
    /// アイテム収納ログ
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    private string CreatePutItemLog(string name, ItemSetup item)
    {
        var sb = new StringBuilder();

        sb.Append(name + "は" + item.ItemName + "を拾った");

        return sb.ToString();
    }

    /// <summary>
    /// アイテム収納失敗ログ
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    private string CreatePutItemFailLog(string name, ItemSetup item)
    {
        var sb = new StringBuilder();

        sb.Append(name + "は" + item.ItemName + "の上に乗った");

        return sb.ToString();
    }
}
