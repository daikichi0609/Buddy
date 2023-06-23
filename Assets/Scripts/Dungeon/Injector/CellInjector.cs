using System.ComponentModel;
using UnityEngine;
using Zenject;

public interface IInjector
{
    void Inject(DiContainer diContainer, GameObject target);
}

public abstract class Injector : IInjector
{
    protected abstract void Inject(DiContainer diContainer, GameObject target);
    void IInjector.Inject(DiContainer diContainer, UnityEngine.GameObject target) => Inject(diContainer, target);
}

public class CellInjector : Injector
{
    protected override void Inject(DiContainer diContainer, GameObject target)
    {
        diContainer.InstantiateComponent<CellInfoHandler>(target);
        diContainer.InstantiateComponent<CellTrapHandler>(target);
    }
}