using System;
using System.Collections;
using System.Collections.Generic;

namespace MyCollections
{
    public class FlexArray<T> : IEnumerable<T>
    {
        protected T[] Data;

        public int Count = 0;

        public int Capacity;

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
            Count = src.Count;
        }

        protected void Init(int capa)
        {
            Capacity = capa;
            Data = new T[Capacity];
            Count = 0;
        }

        public int Add(T v)
        {
            if (Count >= Data.Length)
            {
                Capacity = Data.Length * 2;
                Array.Resize<T>(ref Data, Capacity);
            }

            Data[Count] = v;
            Count++;

            return Count - 1;
        }

        public void Clear()
        {
            Count = 0;
        }

        public T this[int idx]
        {
            get
            {
                return Data[idx];
            }
            set
            {
                Data[idx] = value;
            }
        }

        public ref T Ref(int idx)
        {
            return ref Data[idx];
        }

        public T End()
        {
            return Data[Count - 1];
        }

        public void RemoveAt(int idx)
        {
            Array.Copy(Data, idx + 1, Data, idx, Count - (idx + 1));
            Count--;
        }

        public void ForEach(Action<T> d)
        {
            for (int i = 0; i < Count; i++)
            {
                d(Data[i]);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            int i = 0;

            for (; i < Count; i++)
            {
                yield return Data[i];
            }

            yield break;
        }

        public void RemoveAll(Predicate<T> match)
        {
            int i = Count - 1;
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
            int cnt = Count + src.Count;

            if (cnt >= Data.Length)
            {
                Capacity = cnt * 3 / 2;
                Array.Resize<T>(ref Data, Capacity);
            }

            Array.Copy(src.Data, 0, Data, Count, src.Count);

            Count += src.Count;
        }

        // List空のコピーは少し最適化
        public void AddRange(List<T> src)
        {
            int cnt = Count + src.Count;

            if (cnt >= Data.Length)
            {
                Capacity = cnt * 3 / 2;
                Array.Resize<T>(ref Data, Capacity);
            }

            for (int i=0; i<src.Count; i++)
            {
                Data[Count + i] = src[i];
            }

            Count += src.Count;
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
            if (Count >= Data.Length)
            {
                Capacity = Data.Length * 2;
                Array.Resize<T>(ref Data, Capacity);
            }

            Array.Copy(Data, idx, Data, idx + 1, Count - idx);

            Count++;

            Data[idx] = val;
        }

        public void RemoveRange(int s, int cnt)
        {
            Array.Copy(Data, s + cnt, Data, s, Count - (s + cnt));
            Count -= cnt;
        }

        public void InsertRange(int idx, FlexArray<T> src)
        {
            int cnt = Count + src.Count;
            if (cnt >= Data.Length)
            {
                Capacity = cnt * 3 / 2;
                Array.Resize<T>(ref Data, Capacity);
            }

            Array.Copy(Data, idx, Data, idx + src.Count, Count - idx);
            Array.Copy(src.Data, 0, Data, idx, src.Count);

            Count += src.Count;
        }

        public T Find(Predicate<T> match)
        {
            int i = Count - 1;
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
            int j = Count-1;
            for (;i<j; i++, j--)
            {
                T work = Data[i];

                Data[i] = Data[j];
                Data[j] = work;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            int i = 0;

            for (; i < Count; i++)
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
}
