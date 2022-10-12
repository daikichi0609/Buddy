using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool: SingletonMonoBehaviour<ObjectPool>
{
    public Dictionary<string, List<GameObject>> ObjectPoolDictionary
    {
        get;
    } = new Dictionary<string, List<GameObject>>();

    public GameObject PoolObject(string key)
    {
        ObjectPoolDictionary.TryGetValueEx(key, new List<GameObject>());
        if (ObjectPoolDictionary[key] == null || ObjectPoolDictionary[key].Count == 0)
        {
            return null;
        }

        List<GameObject> list = ObjectPoolDictionary[key];
        GameObject obj = list[list.Count - 1];
        obj.SetActive(true);
        list.RemoveAt(list.Count - 1);
        return obj;
    }

    public void SetObject(string key, GameObject gameObject)
    {
        ObjectPoolDictionary.TryGetValueEx(key, new List<GameObject>());
        gameObject.SetActive(false);
        gameObject.transform.position = new Vector3(0, 0, 0);

        List<GameObject> list = ObjectPoolDictionary[key];
        if (list == null)
        {
            list = new List<GameObject>();
            ObjectPoolDictionary.Add(key, list);
        }
        list.Add(gameObject);
    }
}
