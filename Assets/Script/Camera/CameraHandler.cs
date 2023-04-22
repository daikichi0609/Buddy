using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICameraHandler : ISingleton
{
    void SetParent(GameObject parent);
}

public class CameraHandler : Singleton<CameraHandler, ICameraHandler>, ICameraHandler
{
    /// <summary>
    /// メインカメラ
    /// </summary>
    [SerializeField]
    private GameObject m_MainCamera;

    private static readonly Vector3 ms_KeepPos = new Vector3(0, 5f, -3f);

    private static readonly Vector3 ms_Angle = new Vector3(60f, 0, 0);

    void ICameraHandler.SetParent(GameObject parent)
    {
        m_MainCamera.transform.SetParent(parent.transform);
        m_MainCamera.transform.localPosition = ms_KeepPos;
        m_MainCamera.transform.eulerAngles = ms_Angle;
    }
}
