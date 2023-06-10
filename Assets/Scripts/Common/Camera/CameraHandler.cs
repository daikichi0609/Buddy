using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;

public interface ICameraHandler
{
    IDisposable SetParent(GameObject parent);
}

public class CameraHandler : MonoBehaviour, ICameraHandler
{
    /// <summary>
    /// メインカメラ
    /// </summary>
    [SerializeField]
    private GameObject m_MainCamera;

    private static readonly Vector3 ms_KeepPos = new Vector3(0, 5f, -3f);

    private static readonly Vector3 ms_Angle = new Vector3(60f, 0, 0);

    /// <summary>
    /// カメラを追従させる
    /// </summary>
    /// <param name="parent"></param>
    /// <returns></returns>
    IDisposable ICameraHandler.SetParent(GameObject parent)
    {
        m_MainCamera.transform.SetParent(parent.transform);
        m_MainCamera.transform.localPosition = ms_KeepPos;
        m_MainCamera.transform.eulerAngles = ms_Angle;

        return Disposable.CreateWithState(this, self => self.m_MainCamera.transform.parent = null);
    }
}
