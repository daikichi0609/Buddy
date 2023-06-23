using UnityEngine;
using Zenject;

public class EnemyInjector : UnitInjector
{
    protected override void Inject(DiContainer diContainer, GameObject target)
    {
        base.Inject(diContainer, target);
        diContainer.InstantiateComponent<EnemyAi>(target);
    }
}