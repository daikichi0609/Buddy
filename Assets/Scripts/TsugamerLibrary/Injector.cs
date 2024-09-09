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