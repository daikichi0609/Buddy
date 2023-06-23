using UnityEngine;
using Zenject;

public class FriendInjector : UnitInjector
{
    protected override void Inject(DiContainer diContainer, GameObject target)
    {
        base.Inject(diContainer, target);
        diContainer.InstantiateComponent<FriendAi>(target);
        diContainer.InstantiateComponent<CharaAutoRecovery>(target);
    }
}