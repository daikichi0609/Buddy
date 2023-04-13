using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(menuName = "MyScriptable/Item/Setup")]
public class ItemSetup : ScriptableObject
{
    /// <summary>
    /// プレハブ
    /// </summary>
    [SerializeField]
    private GameObject m_Prefab;
    public GameObject Prefab => m_Prefab;
}
