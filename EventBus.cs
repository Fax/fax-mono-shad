
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;

public interface IGameEvent
{
    public int EventID { get; }
}


public class EventBus
{

    public readonly Queue<IGameEvent> Queue = new();
    public readonly Dictionary<Type, List<Delegate>> Subscribers = new();

    public void Publish<T>(T evt) where T : IGameEvent => Queue.Enqueue(evt);
    public void Subscribe<T>(Action<T> h) where T : IGameEvent
    {
        if (Subscribers.TryGetValue(typeof(T), out var list))
        {
            list.Add(h);
        }
        else
        {
            Subscribers[typeof(T)] = [h];
        }

    }


    public void Drain()
    {
        while (Queue.Count > 0)
        {
            var e = Queue.Dequeue();
            var t = e.GetType();
            if (Subscribers.TryGetValue(t, out var list))
                // the list could be updated with new listeners.
                // and old listeners could be removed.
                // it's very hard to loop on an array that could change.
                // we either defer the changes to the end of the Drain
                // or we lock in place this list until the end of the drain

                // if an object gets removed from the list of subscribers (it dies)
                // while looping, we could end up with an invalid reference.
                for (var i = 0; i < list.Count; i++)
                {
                    var d = list[i];
                    if (d == null) continue;
                    Console.WriteLine($"The target of invocation = {d?.Target?.ToString()}");
                    try
                    {
                        d?.DynamicInvoke(e);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message} \n {ex.StackTrace}");

                    }
                }
        }
    }
}