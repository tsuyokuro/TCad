
using vcompo_t = System.Single;
using vector3_t = OpenTK.Mathematics.Vector3;
using vector4_t = OpenTK.Mathematics.Vector4;
using matrix4_t = OpenTK.Mathematics.Matrix4;

namespace TCad.Util;

/// <summary>
/// 
/// Fast Ring buffer
/// Buffer size Adjusted to power of 2.
/// e.g. 2, 4, 8, 16, 32 ....
/// 
/// </summary>
/// <typeparam name="T"></typeparam>
public class FastRingBuffer<T>
{
    private T[] Data;
    private int Top = 0;
    private int Bottom = 0;
    private int Mask;

    public T this[int i] => Data[(Top + i) & Mask];

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

    public FastRingBuffer(int size)
    {
        CreateBuffer(size);
    }

    public FastRingBuffer()
    {
    }

    public void CreateBuffer(int size)
    {
        BufferSize = Pow2((uint)size);
        Data = new T[BufferSize];
        Mask = BufferSize - 1;
    }

    public void Clear()
    {
        Top = 0;
        Bottom = 0;
        Count = 0;
    }

    static int Pow2(uint n)
    {
        --n;
        int p = 0;
        for (; n != 0; n >>= 1)
        {
            p = (p << 1) + 1;
        }

        return p + 1;
    }

    public void Add(T elem)
    {
        Data[Bottom] = elem;
        Bottom = (Bottom + 1) & Mask;

        if (Count < BufferSize)
        {
            Count++;
        }
        else
        {
            Top = (Top + 1) & Mask;
        }
    }
}
