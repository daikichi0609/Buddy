using System;
using UnityEngine;
using UniRx;

public interface IActorInterface : IDisposable
{
    /// <summary>
    /// コンポーネント初期化
    /// </summary>
    void Initialize();
}

public interface IActorEvent
{
}

public abstract class ActorComponentBase : MonoBehaviour, IActorInterface
{
    /// <summary>
    /// コレクター
    /// </summary>
    protected ICollector Owner { get; set; }

    /// <summary>
    /// Owner取得
    /// </summary>
    private void Awake()
    {
        Owner = GetComponent<ActorComponentCollector>();
        Register(Owner);
    }

    protected virtual void Register(ICollector owner)
    {
        // コンポーネント登録
    }

    protected virtual void Initialize()
    {
        // コンポーネント初期化
    }
    void IActorInterface.Initialize() => Initialize();

    protected virtual void Dispose()
    {

    }
    void IDisposable.Dispose() => Dispose();
}