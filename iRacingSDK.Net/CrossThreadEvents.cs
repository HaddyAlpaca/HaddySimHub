﻿namespace iRacingSDK;

public class CrossThreadEvents<T1, T2>
{
    event Action<T1, T2> evnt;

    Dictionary<Action<T1, T2>, Action<T1, T2>> evntDelegates = new Dictionary<Action<T1, T2>, Action<T1, T2>>();

    public void Invoke(T1 t1, T2 t2)
    {
        evnt?.Invoke(t1, t2);
    }

    public event Action<T1, T2> Event
    {
        add
        {
            var context = SynchronizationContext.Current;
            Action<T1, T2> newDelgate;

            if (context != null)
                newDelgate = (t1, t2) => context.Send(i => value(t1, t2), null);
            else
                newDelgate = value;

            evntDelegates.Add(value, newDelgate);
            evnt += newDelgate;
        }

        remove
        {
            var context = SynchronizationContext.Current;

            var delgate = evntDelegates[value];
            evntDelegates.Remove(value);

            evnt -= delgate;
        }
    }
}

public class CrossThreadEvents<T>
{
    event Action<T> evnt;

    Dictionary<Action<T>, Action<T>> evntDelegates = new Dictionary<Action<T>, Action<T>>();

    public void Invoke(T t)
    {
        evnt?.Invoke(t);
    }

    public event Action<T> Event 
    {
        add
        {
            var context = SynchronizationContext.Current;
            Action<T> newDelgate;

            if (context != null)
                newDelgate = (d) => context.Send(i => value(d), null);
            else
                newDelgate = value;

            evntDelegates.Add(value, newDelgate);
            evnt += newDelgate;
        }

        remove
        {
            var context = SynchronizationContext.Current;

            var delgate = evntDelegates[value];
            evntDelegates.Remove(value);

            evnt -= delgate;
        }
    }
}

public class CrossThreadEvents
{
    event Action evnt;

    Dictionary<Action, Action> evntDelegates = new Dictionary<Action, Action>();

    public void Invoke()
    {
        evnt?.Invoke();
    }
    public event Action Event
    {
        add
        {
            var context = SynchronizationContext.Current;
            Action newDelgate;

            if (context != null)
                newDelgate = () => context.Send(i => value(), null);
            else
                newDelgate = value;

            evntDelegates.Add(value, newDelgate);
            evnt += newDelgate;
        }

        remove
        {
            var context = SynchronizationContext.Current;

            var delgate = evntDelegates[value];
            evntDelegates.Remove(value);

            evnt -= delgate;
        }
    }
}
