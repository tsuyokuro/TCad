using System;
using System.Collections;
using System.Collections.Generic;

namespace MyCollections;

public class FlexArray<T> : IEnumerable<T>
{
    public T[] Data;

    protected int Count_ = 0;

    protected int Capacity_;


    public int Count
    {
        get => Count_;
    }

    public int Capacity
    {
        get => Capacity_;
    }

    public FlexArray()
    {
        Init(8);
    }

    public FlexArray(int capa)
    {
        Init(capa);
    }

    public FlexArray(FlexArray<T> src)
    {
        Init(src.Count);
        Array.Copy(src.Data, Data, src.Count);
        Count_ = src.Count;
    }

    public FlexArray(T[] src)
    {
        Init(src.Length);
        Array.Copy(src, Data, src.Length);
        Count_ = src.Length;
    }

    protected void Init(int capa)
    {
        Capacity_ = capa;
        Data = new T[Capacity_];
        Count_ = 0;
    }

    public int Add(T v)
    {
        if (Count_ >= Data.Length)
        {
            Capacity_ = Data.Length * 2;
            Array.Resize<T>(ref Data, Capacity_);
        }

        Data[Count_] = v;
        Count_++;

        return Count_ - 1;
    }

    public void Clear()
    {
        Count_ = 0;
    }

    public ref T this[int idx]
    {
        get
        {
            return ref Data[idx];
        }
        //set
        //{
        //    Data[idx] = value;
        //}
    }

    public ref T Ref(int idx)
    {
        return ref Data[idx];
    }

    public ref T End()
    {
        return ref Data[Count_ - 1];
    }

    public T Get(int idx)
    {
        return Data[idx];
    }

    public void Set(int idx, T val)
    {
        Data[idx] = val;
    }

    public void RemoveAt(int idx)
    {
        Array.Copy(Data, idx + 1, Data, idx, Count_ - (idx + 1));
        Count_--;
    }

    public void ForEach(Action<T> d)
    {
        for (int i = 0; i < Count_; i++)
        {
            d(Data[i]);
        }
    }


    public void RemoveAll(Predicate<T> match)
    {
        int i = Count_ - 1;
        for (; i >= 0; i--)
        {
            if (match(Data[i]))
            {
                RemoveAt(i);
            }
        }
    }

    public void AddRange(FlexArray<T> src)
    {
        int cnt = Count_ + src.Count;

        if (cnt >= Data.Length)
        {
            Capacity_ = cnt * 3 / 2;
            Array.Resize<T>(ref Data, Capacity_);
        }

        Array.Copy(src.Data, 0, Data, Count_, src.Count);

        Count_ += src.Count;
    }

    // List空のコピーは少し最適化
    public void AddRange(List<T> src)
    {
        int cnt = Count_ + src.Count;

        if (cnt >= Data.Length)
        {
            Capacity_ = cnt * 3 / 2;
            Array.Resize<T>(ref Data, Capacity_);
        }

        for (int i=0; i<src.Count; i++)
        {
            Data[Count_ + i] = src[i];
        }

        Count_ += src.Count;
    }

    public void AddRange(IEnumerable<T> src)
    {
        foreach (T v in src)
        {
            Add(v);
        }
    }

    public void AddRange(IList<T> src)
    {
        foreach (T v in src)
        {
            Add(v);
        }
    }

    public void Insert(int idx, T val)
    {
        if (Count_ >= Data.Length)
        {
            Capacity_ = Data.Length * 2;
            Array.Resize<T>(ref Data, Capacity_);
        }

        Array.Copy(Data, idx, Data, idx + 1, Count_ - idx);

        Count_++;

        Data[idx] = val;
    }

    public void RemoveRange(int s, int cnt)
    {
        Array.Copy(Data, s + cnt, Data, s, Count_ - (s + cnt));
        Count_ -= cnt;
    }

    public void InsertRange(int idx, FlexArray<T> src)
    {
        int cnt = Count_ + src.Count;
        if (cnt >= Data.Length)
        {
            Capacity_ = cnt * 3 / 2;
            Array.Resize<T>(ref Data, Capacity_);
        }

        Array.Copy(Data, idx, Data, idx + src.Count, Count_ - idx);
        Array.Copy(src.Data, 0, Data, idx, src.Count);

        Count_ += src.Count;
    }

    public T Find(Predicate<T> match)
    {
        int i = Count_ - 1;
        for (; i >= 0; i--)
        {
            if (match(Data[i]))
            {
                return Data[i];
            }
        }

        return default(T);
    }

    public void Reverse()
    {
        int i = 0;
        int j = Count_ - 1;
        for (;i<j; i++, j--)
        {
            T work = Data[i];

            Data[i] = Data[j];
            Data[j] = work;
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        int i = 0;

        for (; i < Count_; i++)
        {
            yield return Data[i];
        }

        yield break;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        int i = 0;

        for (; i < Count_; i++)
        {
            yield return Data[i];
        }

        yield break;
    }

    public List<T> ToList()
    {
        List<T> list = new List<T>();

        list.AddRange(this);

        return list;
    }
}
