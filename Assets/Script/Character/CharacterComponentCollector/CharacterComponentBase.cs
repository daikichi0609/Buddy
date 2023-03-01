using System;
using UnityEngine;
using UniRx;

public interface ICharacterInterface : IDisposable
{
    /// <summary>
    /// コンポーネント初期化
    /// </summary>
    void Initialize();
}

public interface ICharacterEvent
{
}

public abstract class CharaComponentBase : MonoBehaviour, ICharacterInterface
{
    /// <summary>
    /// コレクター
    /// </summary>
    protected ICollector Owner { get; set; }

    /// <summary>
    /// Disposeするもの
    /// </summary>
    protected CompositeDisposable Disposable { get; } = new CompositeDisposable();

    /// <summary>
    /// Owner取得
    /// </summary>
    private void Awake()
    {
        Owner = GetComponent<CharacterComponentCollector>();
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
    void ICharacterInterface.Initialize() => Initialize();

    protected virtual void Dispose()
    {
        // コンポーネント破棄
        Disposable.Clear();
    }
    void IDisposable.Dispose() => Dispose();
}