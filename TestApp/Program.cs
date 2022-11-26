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

namespace TestApp;


internal class Program
{
    static void test002()
    {
        CadVertexAttr attr1 = new CadVertexAttr();

        attr1.IsColor1Valid = true;

        CadVertexAttr attr2 = attr1;

        attr1.IsColor1Valid = false;

        Console.WriteLine("attr2.IsColor1Valid:" + attr2.IsColor1Valid);

        VertexList vl1 = new VertexList();
        vl1.Add(new CadVertex(10, 20, 30));

        VertexList vl2 = new VertexList(vl1);

        vl1[0] *= 10;

        Console.WriteLine("vl2[0]:" + vl2[0]);
    }
    static void test003()
    {
        VertexList vl = new VertexList(1000);
        List<CadVertex> list = new List<CadVertex>(1000);

        for (int i=0; i<1000; i++)
        {
            CadVertex v = new CadVertex(i, 0, 0);

            vl.Add(v);
            list.Add(v);
        }

        Thread.Sleep(200);

        Stopwatch sw = new Stopwatch();

        sw.Reset();
        sw.Start();

        for (int j = 0; j < 100000; j++)
        {
            for (int i = 0; i < 1000; i++)
            {
                Vector3d v = list[i].vector;
            }
        }

        sw.Stop();
        Console.WriteLine("List:" + sw.ElapsedMilliseconds);

        sw.Reset();
        sw.Start();

        for (int j = 0; j < 100000; j++)
        {
            foreach (CadVertex v in list)
            {
            }
        }

        sw.Stop();
        Console.WriteLine("List foreach:" + sw.ElapsedMilliseconds);

        sw.Reset();
        sw.Start();

        int num = vl.Count;

        for (int j = 0; j < 100000; j++)
        {
            for (int i = 0; i < num; i++)
            {
                Vector3d v = vl.Data[i].vector;
            }
        }

        sw.Stop();
        Console.WriteLine("VertrxList Data:" + sw.ElapsedMilliseconds);


        sw.Reset();
        sw.Start();

        for (int j = 0; j < 100000; j++)
        {
            for (int i = 0; i < 1000; i++)
            {
                Vector3d v = vl[i].vector;
            }
        }

        sw.Stop();
        Console.WriteLine("VertrxList:" + sw.ElapsedMilliseconds);
    }

    private static void test004()
    {
    }


    static void Main(string[] args)
    {
        test004();
        Console.WriteLine("<<< END >>>");
        Console.ReadLine();
    }
}
