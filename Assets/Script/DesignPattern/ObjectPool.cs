using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool: Singleton<ObjectPool>
{
    public Dictionary<string, List<GameObject>> ObjectPoolDictionary { get; } = new Dictionary<string, List<GameObject>>();

    public bool TryGetPoolObject(string key, out GameObject gameObject)
    {
        gameObject = null;

        ObjectPoolDictionary.TryGetValueEx(key, new List<GameObject>());
        if (ObjectPoolDictionary[key] == null || ObjectPoolDictionary[key].Count == 0)
        {
            return false;
        }

        List<GameObject> list = ObjectPoolDictionary[key];
        gameObject = list[list.Count - 1];
        gameObject.SetActive(true);
        list.RemoveAt(list.Count - 1);
        return true;
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
