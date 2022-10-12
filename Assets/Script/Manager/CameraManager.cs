using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : SingletonMonoBehaviour<CameraManager>
{
    [SerializeField] private GameObject m_MainCamera;
    public GameObject MainCamera
    {
        get { return m_MainCamera; }
        set { m_MainCamera = value; }
    }

    [SerializeField] private Vector3 m_KeepPos = new Vector3(0, 5f, -3f);
    public Vector3 KeepPos
    {
        get { return m_KeepPos; }
    }
    [SerializeField] private Vector3 m_Angle = new Vector3(60f, 0, 0);
    public Vector3 Angle
    {
        get { return m_Angle; }
    }
}
