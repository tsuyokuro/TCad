using System;
using System.Collections.Generic;
using System.Threading;


using vcompo_t = System.Single;
using vector3_t = OpenTK.Mathematics.Vector3;
using vector4_t = OpenTK.Mathematics.Vector4;
using matrix4_t = OpenTK.Mathematics.Matrix4;

namespace TCad.Util;

public class BlockingQueue<T>
{
    public delegate void Removed(T item);

    private readonly List<T> Queue;
    private readonly int Capacity;

    private bool Closing = false;

    public int Count
    {
        get
        {
            lock (Queue)
            {
                return Queue.Count;
            }
        }
    }


    public BlockingQueue(int capacity)
    {
        Capacity = capacity;
        Queue = new List<T>(capacity);
    }

    public void Close()
    {
        Closing = true;
        Monitor.PulseAll(Queue);
    }

    public void Push(T item)
    {
        lock (Queue)
        {
            while (Queue.Count >= Capacity)
            {
                if (Closing)
                {
                    return;
                }

                Monitor.Wait(Queue);
            }

            Queue.Add(item);

            if (Queue.Count == 1)
            {
                Monitor.PulseAll(Queue);
            }
        }
    }
    public T Pop()
    {
        lock (Queue)
        {
            while (Queue.Count == 0)
            {
                if (Closing)
                {
                    return default;
                }

                Monitor.Wait(Queue);
            }

            T item = Queue[0];
            Queue.RemoveAt(0);

            if (Queue.Count == Capacity - 1)
            {
                Monitor.PulseAll(Queue);
            }

            return item;
        }
    }

    public Object GetLock()
    {
        return Queue;
    }

    public int RemoveAll(Predicate<T> match, Removed removed = null)
    {
        int rc = 0;

        lock (Queue)
        {
            for (int i = Queue.Count - 1; i >= 0; i--)
            {
                T item = Queue[i];
                if (match(item))
                {
                    Queue.RemoveAt(i);

                    if (removed != null)
                    {
                        removed(item);
                    }
                }
            }

            if (Queue.Count == Capacity - 1)
            {
                Monitor.PulseAll(Queue);
            }
        }

        return rc;
    }
}
