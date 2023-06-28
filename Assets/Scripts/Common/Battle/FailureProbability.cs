using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public readonly struct FailureTicket<T>
{
    public float Prob { get; }
    public Action<T> OnFail { get; }

    public FailureTicket(float prob, Action<T> onFail)
    {
        Prob = prob;
        OnFail = onFail;
    }

    public static FailureTicket<T> Empty => new FailureTicket<T>();
}

public class FailureProbabilitySystem<T>
{
    private List<FailureTicket<T>> m_FailureTicket = new List<FailureTicket<T>>();

    public IDisposable Register(FailureTicket<T> ticket)
    {
        m_FailureTicket.Add(ticket);
        return Disposable.CreateWithState(this, self => m_FailureTicket.Remove(ticket));
    }

    public bool Judge(out FailureTicket<T> t)
    {
        foreach (var ticket in m_FailureTicket)
            if (ProbabilityCalclator.DetectFromPercent(ticket.Prob * 100) == true)
            {
                t = ticket;
                return true;
            }

        t = FailureTicket<T>.Empty;
        return false;
    }
}
