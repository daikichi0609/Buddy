using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public static class MainThreadCaller
{
    public static void Run(Action action)
    {
        var context = SynchronizationContext.Current;
        context.Post((state) => action.Invoke(), null);
    }
}
