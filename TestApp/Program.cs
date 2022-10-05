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
using Plotter;
using System.Timers;
using System.Collections.Concurrent;

namespace TestApp;

public class MyEvent : EventSequencer<MyEvent>.Event
{
    int Value;
}

internal class Program
{

    static void test002()
    {
        Console.WriteLine("");
    }

    static void Main(string[] args)
    {
        test002();
        Console.WriteLine("<<< END >>>");
        Console.ReadLine();
    }
}
