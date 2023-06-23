using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class UnitInjector : Injector
{
    protected override void Inject(DiContainer diContainer, GameObject target)
    {
        diContainer.InstantiateComponent<CharaBattle>(target);
        diContainer.InstantiateComponent<CharaCellEventChecker>(target);
        diContainer.InstantiateComponent<CharaInventory>(target);
        diContainer.InstantiateComponent<CharaLog>(target);
        diContainer.InstantiateComponent<CharaMove>(target);
        diContainer.InstantiateComponent<CharaSound>(target);
        diContainer.InstantiateComponent<CharaStatus>(target);
        diContainer.InstantiateComponent<CharaTurn>(target);
        diContainer.InstantiateComponent<CharaTypeHolder>(target);
        diContainer.InstantiateComponent<CharaLastActionHolder>(target);
    }
}