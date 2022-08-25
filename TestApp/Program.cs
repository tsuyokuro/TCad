using MessagePack;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Collections;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Text;

namespace TestApp
{
    public class FastRingBuffer<T>
    {
        public T[] Data;
        public int Top = 0;
        public int Bottom = 0;
        public int Mask;

        public T this[int i] => Data[(i + Top) & Mask];

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

        public static int Pow2(uint n)
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

    public class FileUtil
    {
        public static string GetExternalDataDir(string fname)
        {
            string path = fname + "_external";
            return path;
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            string fname = @"F:\work\test.tcad";

            string epath =  FileUtil.GetExternalDataDir(fname);

            Directory.CreateDirectory(epath);

            string dname = Path.GetFileName(epath);
        }
    }
}
