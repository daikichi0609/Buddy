using UnityEngine;
using System;

/// <summary>
/// インターフェイスを使わない
/// </summary>
/// <typeparam name="T"></typeparam>
/// <typeparam name="IT"></typeparam>
public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T ms_Instance;
    public static T Instance
    {
        get
        {
            if (ms_Instance == null)
            {
                Type t = typeof(T);

                ms_Instance = (T)FindObjectOfType(t);

                if (ms_Instance == null)
                {
                    Debug.LogError(t + " をアタッチしているGameObjectはありません");
                }
            }

            return ms_Instance;
        }
    }

    virtual protected void Awake()
    {
        // 他のゲームオブジェクトにアタッチされているか調べる
        // アタッチされている場合は破棄する。
        CheckInstance();
    }

    protected bool CheckInstance()
    {
        if (ms_Instance == null)
        {
            ms_Instance = this as T;
            return true;
        }
        else if (Instance == this)
        {
            return true;
        }

        Destroy(this);
        return false;
    }
}

public interface ISingleton
{

}

/// <summary>
/// インターフェイスへの参照をキャッシュする
/// </summary>
/// <typeparam name="T"></typeparam>
/// <typeparam name="IT"></typeparam>
public abstract class Singleton<T, IT> : MonoBehaviour where T : MonoBehaviour, ISingleton where IT : class, ISingleton
{
    private static T ms_Instance;
    public static T Instance
    {
        get
        {
            if (ms_Instance == null)
            {
                Type t = typeof(T);

                ms_Instance = (T)FindObjectOfType(t);

                if (ms_Instance == null)
                {
                    Debug.LogError(t + " をアタッチしているGameObjectはありません");
                }
            }

            return ms_Instance;
        }
    }

    public static IT Interface
    {
        get
        {
            if (Instance == null)
                return null;

            return Instance as IT;
        }
    }

    virtual protected void Awake()
    {
        // 他のゲームオブジェクトにアタッチされているか調べる
        // アタッチされている場合は破棄する。
        CheckInstance();
    }

    protected bool CheckInstance()
    {
        if (ms_Instance == null)
        {
            ms_Instance = this as T;
            return true;
        }
        else if (Instance == this)
        {
            return true;
        }

        Destroy(this);
        return false;
    }
}