using UnityEngine;
using Zenject;

public class PlayerInjector : UnitInjector
{
    protected override void Inject(DiContainer diContainer, GameObject target)
    {
        base.Inject(diContainer, target);
        diContainer.InstantiateComponent<PlayerInput>(target);
        diContainer.InstantiateComponent<CharaAutoRecovery>(target);
        diContainer.InstantiateComponent<CharaStarvation>(target);
    }
}