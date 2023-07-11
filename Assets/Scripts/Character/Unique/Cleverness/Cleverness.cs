using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using NaughtyAttributes;

public interface ICleverness
{
    /// <summary>
    /// かしこさ名
    /// </summary>
    string Name { get; }

    /// <summary>
    /// かしこさ説明
    /// </summary>
    string Description { get; }

    /// <summary>
    /// 切り替え可能か
    /// </summary>
    bool CanSwitch { get; }

    /// <summary>
    /// 有効化
    /// </summary>
    /// <param name="ctx"></param>
    /// <returns></returns>
    IDisposable Activate(ClevernessContext ctx);
}

public abstract class Cleverness : ScriptableObject, ICleverness
{
    [ShowNativeProperty]
    protected abstract string Name { get; }
    string ICleverness.Name => Name;

    [ShowNativeProperty]
    protected abstract string Description { get; }
    string ICleverness.Description => Description;

    [ShowNativeProperty]
    protected abstract bool CanSwitch { get; }
    bool ICleverness.CanSwitch => CanSwitch;

    protected abstract IDisposable Activate(ClevernessContext ctx);
    IDisposable ICleverness.Activate(ClevernessContext ctx) => Activate(ctx);
}

public readonly struct ClevernessContext
{
    /// <summary>
    /// かしこさ保有者
    /// </summary>
    public ICollector Owner { get; }

    public ClevernessContext(ICollector owner)
    {
        Owner = owner;
    }
}