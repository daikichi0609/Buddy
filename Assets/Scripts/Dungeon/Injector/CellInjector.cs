using System.ComponentModel;
using UnityEngine;
using Zenject;

public class CellInjector : Injector
{
    protected override void Inject(DiContainer diContainer, GameObject target)
    {
        diContainer.InstantiateComponent<CellInfoHandler>(target);
        diContainer.InstantiateComponent<CellTrapHandler>(target);
    }
}