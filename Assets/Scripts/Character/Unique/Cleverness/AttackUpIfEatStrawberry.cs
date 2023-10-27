using System;
using UniRx;
using UnityEngine;

public class AttackUpIfEatStrawberry : Cleverness
{
    protected override string Name => "ベリベリパワー";
    protected override string Description => "いちごを食べると、少しの間だけ攻撃力が上昇する。";
    protected override bool CanSwitch => true;

    [SerializeField]
    private ItemSetup m_ItemSetup;

    protected override IDisposable Activate(ClevernessContext ctx)
    {
        return ctx.Owner.GetInterface<ICharaEatEffect>().SetFoodAttackUp(m_ItemSetup);
    }
}