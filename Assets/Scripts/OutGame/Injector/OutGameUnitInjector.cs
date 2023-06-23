using UnityEngine;
using Zenject;

public class OutGameUnitInjector : Injector
{
    protected override void Inject(DiContainer diContainer, GameObject target)
    {
        diContainer.InstantiateComponent<CharaTalk>(target);
    }
}