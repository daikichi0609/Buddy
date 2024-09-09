using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public interface IObjectPoolController
{
    /// <summary>
    /// 汎用プール操作
    /// </summary>
    /// <param name="key"></param>
    /// <param name="gameObject"></param>
    /// <returns></returns>
    bool TryGetObject(string key, out GameObject gameObject);
    void SetObject(string key, GameObject gameObject);

    /// <summary>
    /// セットアップがあるGameObject
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="setup"></param>
    /// <returns></returns>
    GameObject GetObject<T>(T setup) where T : PrefabSetup;

    /// <summary>
    /// セットアップとInjectorがあるGameObject
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="setup"></param>
    /// <returns></returns>
    GameObject GetObject<T>(T setup, IInjector injector) where T : PrefabSetup;
    void SetObject<T>(T setup, GameObject gameObject) where T : PrefabSetup;
}

public class ObjectPoolController : IObjectPoolController
{
    [Inject]
    private IInstantiater m_Instantiater;

    /// <summary>
    /// プールインスタンス
    /// </summary>
    private ObjectPool m_ObjectPool = new ObjectPool();

    bool IObjectPoolController.TryGetObject(string key, out GameObject gameObject) => m_ObjectPool.TryGetPoolObject(key, out gameObject);
    void IObjectPoolController.SetObject(string key, GameObject gameObject) => m_ObjectPool.SetObject(key, gameObject);

    /// <summary>
    /// セットアップのオブジェクト取得
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    GameObject IObjectPoolController.GetObject<T>(T setup)
    {
        if (m_ObjectPool.TryGetPoolObject(setup.ToString(), out var chara) == false)
            chara = m_Instantiater.InstantiatePrefab(setup.Prefab);

        return chara;
    }

    /// <summary>
    /// セットアップのオブジェクト取得
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    GameObject IObjectPoolController.GetObject<T>(T setup, IInjector injector)
    {
        if (m_ObjectPool.TryGetPoolObject(setup.ToString(), out var chara) == false)
            chara = m_Instantiater.InstantiatePrefab(setup.Prefab, injector);

        return chara;
    }

    /// <summary>
    /// セットアップのオブジェクトセット
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="setup"></param>
    /// <param name="gameObject"></param>
    void IObjectPoolController.SetObject<T>(T setup, GameObject gameObject) => m_ObjectPool.SetObject(setup.ToString(), gameObject);
}
