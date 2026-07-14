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

using TCad.Controls.CadConsole;


using vdata_t = System.Single;


namespace TestApp;

internal class Program
{

    private static void test001()
    {
        TextRange sel = new(new TextPos(0, 5), new TextPos(0, 8));

        var span0 = sel.GetRowSpan(0);
        var span1 = sel.GetRowSpan(1);
        var span2 = sel.GetRowSpan(2);
        var span3 = sel.GetRowSpan(3);
    }


    static void Main(string[] args)
    {
        test001();
        Console.ReadLine();
    }
}
