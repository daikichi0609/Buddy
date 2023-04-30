using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "MyScriptable/OutGame/CheckPoint")]
public class CheckPointSetup : ScriptableObject
{
    [SerializeField, Header("ステージ")]
    private GameObject m_Stage;
    public GameObject Stage => m_Stage;
}
