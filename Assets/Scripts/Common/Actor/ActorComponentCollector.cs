using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
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
    void Register<TComp>(TComp comp) where TComp : class;

    /// <summary>
    /// コンポーネント取得
    /// </summary>
    /// <typeparam name="TComp"></typeparam>
    /// <returns></returns>
    TComp GetInterface<TComp>() where TComp : class, IActorInterface;
    TEvent GetEvent<TEvent>() where TEvent : class, IActorEvent;

    /// <summary>
    /// コンポーネント要求
    /// </summary>
    /// <param name="comp"></param>
    /// <returns></returns>
    bool RequireInterface<TComp>(out TComp comp) where TComp : class, IActorInterface;
    bool RequireEvent<TEvent>(out TEvent comp) where TEvent : class, IActorEvent;

    /// <summary>
    /// 初期化
    /// </summary>
    void Initialize();

    /// <summary>
    /// Disposable
    /// </summary>
    CompositeDisposable Disposables { get; }
}

/// <summary>
/// コンポーネント集約クラス
/// </summary>
[Serializable]
public class ActorComponentCollector : MonoBehaviour, ICollector
{
    private HashSet<IActorInterface> m_Interfaces = new HashSet<IActorInterface>();
    private HashSet<IActorEvent> m_Events = new HashSet<IActorEvent>();

    /// <summary>
    /// Disposeするもの
    /// </summary>
    protected CompositeDisposable m_CompositeDisposable = new CompositeDisposable();
    CompositeDisposable ICollector.Disposables => m_CompositeDisposable;

    /// <summary>
    /// 初期化
    /// </summary>
    private void Initialize()
    {
        foreach (var comp in m_Interfaces)
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
        if (comp is IActorInterface)
            m_Interfaces.Add(comp as IActorInterface);

        if (comp is IActorEvent)
            m_Events.Add(comp as IActorEvent);
    }

    /// <summary>
    /// コンポーネント取得
    /// </summary>
    /// <typeparam name="TComp"></typeparam>
    /// <returns></returns>
    TComp ICollector.GetInterface<TComp>()
    {
        foreach (var val in m_Interfaces)
            if (val is TComp)
                return val as TComp;

        Debug.LogError("コンポーネントの取得に失敗しました");
        return null;
    }

    /// <summary>
    /// イベントコンポーネント取得
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    /// <returns></returns>
    TEvent ICollector.GetEvent<TEvent>()
    {
        foreach (var val in m_Events)
            if (val is TEvent)
                return val as TEvent;

        Debug.LogError("イベントコンポーネントの取得に失敗しました");
        return null;
    }

    /// <summary>
    /// コンポーネント要求
    /// </summary>
    /// <typeparam name="TComp"></typeparam>
    /// <param name="comp"></param>
    /// <returns></returns>
    bool ICollector.RequireInterface<TComp>(out TComp comp)
    {
        foreach (var val in m_Interfaces)
            if (val is TComp)
            {
                comp = val as TComp;
                return true;
            }

        comp = null;
        return false;
    }

    /// <summary>
    /// イベントコンポーネント要求
    /// </summary>
    /// <typeparam name="TComp"></typeparam>
    /// <param name="comp"></param>
    /// <returns></returns>
    bool ICollector.RequireEvent<TEvent>(out TEvent comp)
    {
        foreach (var val in m_Events)
            if (val is TEvent)
            {
                comp = val as TEvent;
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
        foreach (var comp in m_Interfaces)
            comp.Dispose();

        m_CompositeDisposable.Clear();
    }
}
