using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroyOnLoad : MonoBehaviour
{
    private static bool isCreated = false;
    private void Awake()
    {
        if (isCreated == false)
        {
            isCreated = true;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }
}
