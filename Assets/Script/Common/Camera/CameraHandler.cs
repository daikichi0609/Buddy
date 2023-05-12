using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;

public interface ICameraHandler : ISingleton
{
    IDisposable SetParent(GameObject parent);
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

    /// <summary>
    /// 親子関係構築
    /// </summary>
    /// <param name="parent"></param>
    /// <returns></returns>
    IDisposable ICameraHandler.SetParent(GameObject parent)
    {
        m_MainCamera.transform.SetParent(parent.transform);
        m_MainCamera.transform.localPosition = ms_KeepPos;
        m_MainCamera.transform.eulerAngles = ms_Angle;

        return Disposable.Create(() => CancelParent());
    }

    /// <summary>
    /// 親子関係キャンセル
    /// </summary>
    private void CancelParent() => m_MainCamera.transform.parent = null;
}
