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
using System.Diagnostics;
using TCad.Controls;
using OpenTK.Mathematics;
using System.Timers;
using System.Collections.Concurrent;
using TCad.Util;
using CadDataTypes;
using Microsoft.Scripting.Utils;
using GLUtil;


using vdata_t = System.Single;


namespace TestApp;

internal class Program
{

    private static void test004()
    {
        int lcnt = 1000000000;

        {
            vdata_t v = 0;
            vdata_t[] vt = new vdata_t[1024];

            Thread.Sleep(500);

            Stopwatch sw = new Stopwatch();

            sw.Reset();
            sw.Start();
            for (int i = 0; i < vt.Length; i++)
            {
                vt[i] = (vdata_t)0.01 * i;
            }

            for (int i = 0; i < lcnt; i++)
            {
                v += vt[i & (1024 - 1)];
                v = (vdata_t)Math.Sqrt((vdata_t)(v + (vdata_t)0.25));
            }
            sw.Stop();
            Console.WriteLine("vdata_t:" + sw.ElapsedMilliseconds);
            Console.WriteLine("v:" + v);
        }

        {
            Thread.Sleep(500);

            double dv = 0;
            double[] dvt = new double[1024];

            Stopwatch sw = new Stopwatch();
            sw.Reset();
            sw.Start();
            for (int i = 0; i < dvt.Length; i++)
            {
                dvt[i] = (double)0.01 * i;
            }

            for (int i = 0; i < lcnt; i++)
            {
                dv += dvt[i & (1024 - 1)];
                dv = (vdata_t)Math.Sqrt(dv + 0.25);
            }
            sw.Stop();
            Console.WriteLine("double:" + sw.ElapsedMilliseconds);
            Console.WriteLine("dv:" + dv);
        }
    }


    static void Main(string[] args)
    {
        test004();
        Console.WriteLine("<<< END >>>");
        Console.ReadLine();
    }
}
