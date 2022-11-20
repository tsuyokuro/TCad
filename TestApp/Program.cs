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

    static void Main(string[] args)
    {
        test002();
        Console.WriteLine("<<< END >>>");
        Console.ReadLine();
    }
}
