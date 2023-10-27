using System;
using UniRx;
using UnityEngine;

public class GenerateStrawberry : Cleverness
{
    protected override string Name => "いちごをどうぞ";
    protected override string Description => "バッグに空きがあるなら、いちごをバッグの中に１つ入れる";
    protected override bool CanSwitch => true;

    [SerializeField]
    private ItemSetup m_ItemSetup;

    protected override IDisposable Activate(ClevernessContext ctx)
    {
        return ctx.TurnManager.OnTurnStartFirst.SubscribeWithState((ctx.Owner, m_ItemSetup), (_, tuple) =>
        {
            var inventory = tuple.Owner.GetInterface<ICharaInventory>();
            inventory.Put(tuple.m_ItemSetup);
        });
    }
}