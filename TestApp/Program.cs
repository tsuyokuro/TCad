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
using System.Text.RegularExpressions;

namespace TestApp
{
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
            for (int i=0; i < Count; i++)
            {
                action(this[i]);
            }
        }
    }

    internal class Program
    {
        static void test001()
        {
            var rb = new RingBuffer<string>(3);

            for (int i=0; i<20; i++)
            {
                rb.Add("" + (i + 1) + "_abcdefg");
            }

            rb.ForEach((s) =>
            {
                Console.WriteLine(s);
            });
        }

        static void Main(string[] args)
        {
            test001();
            Console.WriteLine("<<< END >>>");
            Console.ReadLine();
        }
    }
}
