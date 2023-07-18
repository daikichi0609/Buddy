using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;

public interface ICameraHandler
{
    /// <summary>
    /// 親子設定
    /// </summary>
    /// <param name="parent"></param>
    /// <returns></returns>
    IDisposable SetParent(GameObject parent);

    /// <summary>
    /// 有効化切り替え
    /// </summary>
    /// <param name="isActive"></param>
    /// <returns></returns>
    IDisposable SetActive(bool isActive);
}

public class CameraHandler : MonoBehaviour, ICameraHandler
{
    /// <summary>
    /// メインカメラ
    /// </summary>
    [SerializeField]
    private GameObject m_MainCamera;

    private static readonly Vector3 ms_KeepPos = new Vector3(0, 5.5f, -3.5f);
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

    /// <summary>
    /// 有効化切り替え
    /// </summary>
    /// <param name="isActive"></param>
    /// <returns></returns>
    IDisposable ICameraHandler.SetActive(bool isActive)
    {
        m_MainCamera.SetActive(isActive);
        return Disposable.CreateWithState((this, isActive), tuple => tuple.Item1.m_MainCamera.SetActive(!tuple.isActive));
    }
}
