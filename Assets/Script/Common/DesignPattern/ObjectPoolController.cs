using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IObjectPoolController : ISingleton
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
    void SetObject<T>(T setup, GameObject gameObject) where T : PrefabSetup;
}

public class ObjectPoolController : Singleton<ObjectPoolController, IObjectPoolController>, IObjectPoolController
{
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
        if (m_ObjectPool.TryGetPoolObject(setup.name, out var chara) == false)
            chara = Instantiate(setup.Prefab);

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
