using System;


using vcompo_t = System.Double;
using vector3_t = OpenTK.Mathematics.Vector3d;
using vector4_t = OpenTK.Mathematics.Vector4d;
using matrix4_t = OpenTK.Mathematics.Matrix4d;

namespace TCad.Util;

public class RingBuffer<T>
{
    private T[] Data;
    private int Top = 0;
    private int Bottom = 0;

    public T this[int i] => Data[(Top + i) % BufferSize];

    public int Count
    {
        get;
        private set;
    }

    public int BufferSize
    {
        get;
        private set;
    }

    public RingBuffer(int size)
    {
        CreateBuffer(size);
    }

    public RingBuffer()
    {
    }

    public void CreateBuffer(int size)
    {
        BufferSize = size;
        Data = new T[BufferSize];
    }

    public void Clear()
    {
        Top = 0;
        Bottom = 0;
        Count = 0;
    }

    public void Add(T elem)
    {
        Data[Bottom] = elem;
        Bottom = (Bottom + 1) % BufferSize;

        if (Count < BufferSize)
        {
            Count++;
        }
        else
        {
            Top = (Top + 1) % BufferSize;
        }
    }

    public void ForEach(Action<T> action)
    {
        for (int i = 0; i < Count; i++)
        {
            action(this[i]);
        }
    }
}
