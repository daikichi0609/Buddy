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
}

public class Instantiater : MonoBehaviour, IInstantiater
{
    [Inject] private DiContainer m_Container;

    /// <summary>
    /// インスタンス生成
    /// </summary>
    /// <param name="prefab"></param>
    /// <returns></returns>
    GameObject IInstantiater.InstantiatePrefab(GameObject prefab) => m_Container.InstantiatePrefab(prefab);
}
