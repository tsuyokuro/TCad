using MessagePack;
using System;
using Plotter;
using CadDataTypes;
using Plotter.Serializer.v1002;
using Plotter.Serializer;
using TCad;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Collections;
using MyCollections;
using OpenTK.Mathematics;

namespace TestApp
{
    public class MpTest : IMessagePackSerializationCallbackReceiver
    {
        [Key("X")]
        public double X;

        [Key("Y")]
        public double Y;

        [IgnoreMember]
        public double L = 0;

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            L = Math.Sqrt(X * X + Y * Y);
        }
    }


    [MessagePackObject]
    public class MpTest1
    {
        [Key("X")]
        public double X;

        [Key("Y")]
        public double Y;
    }

    [MessagePackObject]
    public class MpTest2 : IMessagePackSerializationCallbackReceiver
    {
        [Key("X")]
        public double X;

        [Key("Y")]
        public double Y;

        [Key("Version")]
        public byte[] Version = { 1, 0, 0, 2 };


        [IgnoreMember]
        public double L = 0;

        public void OnBeforeSerialize()
        {
            // NOP
        }

        public void OnAfterDeserialize()
        {
            L = Math.Sqrt(X * X + Y * Y);
        }
    }

    public class FlexArrayW<T> : IEnumerable<T>
    {
        private FlexArray<T> Data;

        public Func<T, T> GetFunc = (v) => v;
        public Func<T, T> SetFunc = (v) => v;

        public FlexArrayW(FlexArray<T> data)
        {
            Data = data;
        }

        public int Add(T v)
        {
            Data.Add(v);
            return Data.Count - 1;
        }

        public void Clear()
        {
            Data.Clear();
        }

        public T this[int idx]
        {
            get
            {
                return GetFunc(Data[idx]);
            }
            set
            {
                Data[idx] = SetFunc(value);
            }
        }

        public ref T Ref(int idx)
        {
            return ref Data.Ref(idx);
        }

        public T End()
        {
            return GetFunc(Data.End());
        }

        public void RemoveAt(int idx)
        {
            Data.RemoveAt(idx);
        }

        public void ForEach(Action<T> d)
        {
            for (int i = 0; i < Data.Count; i++)
            {
                d(GetFunc(Data[i]));
            }
        }

        public void RemoveAll(Predicate<T> match)
        {
            Data.RemoveAll(match);
        }

        public void AddRange(FlexArrayW<T> src)
        {
            Data.AddRange(src.Data);
        }

        public void AddRange(List<T> src)
        {
            Data.AddRange(src);
        }

        public void AddRange(IEnumerable<T> src)
        {
            Data.AddRange(src);
        }

        public void AddRange(IList<T> src)
        {
            Data.AddRange(src);
        }

        public void Insert(int idx, T val)
        {
            Data.Insert(idx, val);
        }

        public void RemoveRange(int s, int cnt)
        {
            Data.RemoveRange(s, cnt);
        }

        public void InsertRange(int idx, FlexArrayW<T> src)
        {
            Data.InsertRange(idx, src.Data);
        }

        public T Find(Predicate<T> match)
        {
            return Data.Find(match);
        }

        public void Reverse()
        {
            Data.Reverse();
        }

        public IEnumerator<T> GetEnumerator()
        {
            int i = 0;

            for (; i < Data.Count; i++)
            {
                yield return GetFunc(Data[i]);
            }

            yield break;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            int i = 0;

            for (; i < Data.Count; i++)
            {
                yield return GetFunc(Data[i]);
            }

            yield break;
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            VertexList list = new VertexList();

            CadVertex v = default;

            CadVertex v1 = new CadVertex(4, 5, 6);

            list.Add(new CadVertex(1, 2, 3));
            list.Add(new CadVertex(1, 2, 3));
            list.Add(new CadVertex(1, 2, 3));

            list[1].Selected = true;

            list[1] = v1;

            v = list[2];

            ref CadVertex vx = ref list[2];

            v.X = 100;

            vx.Z = 100;


            Vector3d tv = (Vector3d)vx;


            Console.WriteLine("End");
        }
    }
}
