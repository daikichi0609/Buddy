using System.Collections;
using System.Collections.Generic;
using Fungus;
using UnityEngine;
using Zenject;

public interface IInstantiater
{
    /// <summary>
    /// インスタンス生成
    /// </summary>
    /// <param name="prefab"></param>
    /// <returns></returns>
    GameObject InstantiatePrefab(GameObject prefab);

    /// <summary>
    /// インスタンス生成
    /// </summary>
    /// <param name="prefab"></param>
    /// <returns></returns>
    GameObject InstantiatePrefab(GameObject prefab, IInjector injector);
}

public class Instantiater : MonoBehaviour, IInstantiater
{
    [Inject]
    private DiContainer m_Container;

    /// <summary>
    /// インスタンス生成
    /// </summary>
    /// <param name="prefab"></param>
    /// <returns></returns>
    private GameObject InstantiatePrefab(GameObject prefab) => m_Container.InstantiatePrefab(prefab);
    GameObject IInstantiater.InstantiatePrefab(GameObject prefab) => InstantiatePrefab(prefab);

    /// <summary>
    /// インスタンス生成
    /// </summary>
    /// <param name="prefab"></param>
    /// <returns></returns>
    GameObject IInstantiater.InstantiatePrefab(GameObject prefab, IInjector injector)
    {
        var gameObject = InstantiatePrefab(prefab);
        injector.Inject(m_Container, gameObject);
        return gameObject;
    }
}
