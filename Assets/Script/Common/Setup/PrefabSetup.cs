using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabSetup : ScriptableObject
{
    /// <summary>
    /// プレハブ
    /// </summary>
    [SerializeField, Header("プレハブ")]
    private GameObject m_Prefab;
    public GameObject Prefab => m_Prefab;
}
