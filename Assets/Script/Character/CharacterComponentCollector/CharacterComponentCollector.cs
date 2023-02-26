using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// コレクターインターフェイス
/// </summary>
public interface ICollector : IDisposable
{
    /// <summary>
    /// コンポーネント登録
    /// </summary>
    /// <param name="comp"></param>
    void Register<TComp>(TComp comp) where TComp : class, ICharacterComponent;

    /// <summary>
    /// コンポーネント取得
    /// </summary>
    /// <typeparam name="TComp"></typeparam>
    /// <returns></returns>
    TComp GetComponent<TComp>() where TComp : class, ICharacterComponent;

    /// <summary>
    /// コンポーネント要求
    /// </summary>
    /// <param name="comp"></param>
    /// <returns></returns>
    bool RequireComponent<TComp>(out TComp comp) where TComp : class, ICharacterComponent;

    /// <summary>
    /// 初期化
    /// </summary>
    void Initialize();
}

/// <summary>
/// コンポーネント集約クラス
/// </summary>
public class CharacterComponentCollector : MonoBehaviour, ICollector, IDisposable
{
    private List<ICharacterComponent> m_Components = new List<ICharacterComponent>();

    /// <summary>
    /// UnityのStart関数
    /// </summary>
    private void Start() => Initialize();

    /// <summary>
    /// 初期化
    /// </summary>
    private void Initialize()
    {
        foreach (var comp in m_Components)
            comp.Initialize();
    }
    void ICollector.Initialize() => Initialize();

    /// <summary>
    /// コンポーネント登録
    /// </summary>
    /// <typeparam name="TComp"></typeparam>
    /// <param name="comp"></param>
    void ICollector.Register<TComp>(TComp comp)
    {
        m_Components.Add(comp);
    }

    /// <summary>
    /// コンポーネント取得
    /// </summary>
    /// <typeparam name="TComp"></typeparam>
    /// <returns></returns>
    TComp ICollector.GetComponent<TComp>()
    {
        foreach (var val in m_Components)
            if (val is TComp)
                return val as TComp;

        Debug.LogError("コンポーネントの取得に失敗しました");
        return null;
    }

    /// <summary>
    /// コンポーネント要求
    /// </summary>
    /// <typeparam name="TComp"></typeparam>
    /// <param name="comp"></param>
    /// <returns></returns>
    bool ICollector.RequireComponent<TComp>(out TComp comp)
    {
        foreach (var val in m_Components)
            if (val is TComp)
            {
                comp = val as TComp;
                return true;
            }

        comp = null;
        return false;
    }

    /// <summary>
    /// 破棄
    /// </summary>
    void IDisposable.Dispose()
    {
        foreach (var comp in m_Components)
            comp.Dispose();
    }
}
