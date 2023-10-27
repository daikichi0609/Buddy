using System;
using UniRx;
using UnityEngine;

public class CriticalRatioUpIfEatStrawberry : Cleverness
{
    protected override string Name => "ベリベリラッキー";
    protected override string Description => "いちごを食べると、少しの間だけクリティカル率が上昇する";
    protected override bool CanSwitch => true;

    [SerializeField]
    private ItemSetup m_ItemSetup;

    protected override IDisposable Activate(ClevernessContext ctx)
    {
        return ctx.Owner.GetInterface<ICharaEatEffect>().SetFoodCriticalRatioUp(m_ItemSetup);
    }
}