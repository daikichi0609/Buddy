using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : Singleton<ObjectPool>
{
    public Dictionary<string, List<GameObject>> ObjectPoolDictionary { get; } = new Dictionary<string, List<GameObject>>();

    public bool TryGetPoolObject(string key, out GameObject gameObject)
    {
        gameObject = null;

        if (ObjectPoolDictionary.TryGetValue(key, out var gameObjects) == false || gameObjects.Count == 0)
            return false;

        List<GameObject> list = ObjectPoolDictionary[key];
        gameObject = list[list.Count - 1];
        gameObject.SetActive(true);
        list.RemoveAt(list.Count - 1);
        return true;
    }

    public void SetObject(string key, GameObject gameObject)
    {
        if (ObjectPoolDictionary.TryGetValue(key, out var list) == false)
            ObjectPoolDictionary.Add(key, new List<GameObject>());

        gameObject.SetActive(false);
        gameObject.transform.position = new Vector3(0, 0, 0);

        list.Add(gameObject);
    }
}
