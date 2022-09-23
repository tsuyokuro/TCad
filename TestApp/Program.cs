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

namespace TestApp
{
    internal class Program
    {
        static void test001()
        {
            TextAttr attr = default;
            attr.FColor = 1;
            attr.BColor = 0;

            TextLine tl = new TextLine(attr);

            string s = "漢字\r" + AnsiEsc.Green + "テスト" + AnsiEsc.Blue + AnsiEsc.GreenBG + "ABCdef";

            tl.Parse(s);

            var sw = new Stopwatch();
            sw.Start();


            foreach (AttrSpan span in tl.Attrs)
            {
                if (span.Len == 0) continue;

                string ps = tl.Data.Substring(span.Start, span.Len);

                Console.WriteLine(ps);
            }

            sw.Stop();

            Console.WriteLine("" + sw.ElapsedMilliseconds);
        }

        static void Main(string[] args)
        {
            Thread.Sleep(100);
            test001();
            Console.WriteLine("<<< END >>>");
            Console.ReadLine();
        }
    }
}
