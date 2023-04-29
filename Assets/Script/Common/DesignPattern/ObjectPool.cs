using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool
{
    public Dictionary<string, List<GameObject>> ObjectPoolDictionary { get; } = new Dictionary<string, List<GameObject>>();

    public bool TryGetPoolObject(string key, out GameObject gameObject)
    {
        gameObject = null;

        var list = ObjectPoolDictionary.GetOrDefault(key, new List<GameObject>());
        if (list.Count == 0)
            return false;

        gameObject = list[list.Count - 1];
        gameObject.SetActive(true);
        list.RemoveAt(list.Count - 1);
        return true;
    }

    public void SetObject(string key, GameObject gameObject)
    {
        var list = ObjectPoolDictionary.GetOrDefault(key, new List<GameObject>());

        gameObject.SetActive(false);
        gameObject.transform.position = new Vector3(0, 0, 0);

        list.Add(gameObject);
    }
}
