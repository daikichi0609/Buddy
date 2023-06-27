using System;
using System.Threading.Tasks;

public static class DelegateExtensions
{

    #region action -> async func

    /// <summary>
    /// Converts the specified action to a async func.
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public static Func<Task> ToAsync(this Action action)
    {
        return () => Task.Run(action);
    }

    /// <summary>
    /// Converts the specified action to a async func.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <param name="action"></param>
    /// <returns></returns>
    public static Func<T1, Task> ToAsync<T1>(this Action<T1> action)
    {
        return (arg1) => Task.Run(() => action(arg1));
    }

    /// <summary>
    /// Converts the specified action to a async func.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <param name="action"></param>
    /// <returns></returns>
    public static Func<T1, T2, Task> ToAsync<T1, T2>(this Action<T1, T2> action)
    {
        return (arg1, arg2) => Task.Run(() => action(arg1, arg2));
    }

    #endregion

    #region func -> async func

    /// <summary>
    /// Converts the specified func to a async func.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="func"></param>
    /// <returns></returns>
    public static Func<Task<TResult>> ToAsync<TResult>(this Func<TResult> func)
    {
        return () => Task.Run(() => func());
    }

    /// <summary>
    /// Converts the specified func to a async func.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="func"></param>
    /// <returns></returns>
    public static Func<T1, Task<TResult>> ToAsync<T1, TResult>(this Func<T1, TResult> func)
    {
        return (arg1) => Task.Run(() => func(arg1));
    }

    /// <summary>
    /// Converts the specified func to a async func.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="func"></param>
    /// <returns></returns>
    public static Func<T1, T2, Task<TResult>> ToAsync<T1, T2, TResult>(this Func<T1, T2, TResult> func)
    {
        return (arg1, arg2) => Task.Run(() => func(arg1, arg2));
    }

    #endregion
}