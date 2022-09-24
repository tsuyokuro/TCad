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

namespace TestApp
{
    [MessagePackObject]
    public struct MpColor4
    {
        [Key(0)]
        public float R;
        [Key(1)]
        public float G;
        [Key(2)]
        public float B;
        [Key(3)]
        public float A;

        public static MpColor4 Create(Color4 c)
        {
            MpColor4 ret = new MpColor4();
            ret.R = c.R;
            ret.G = c.G;
            ret.B = c.B;
            ret.A = c.A;

            return ret;
        }
    }

    [MessagePackObject]
    public class Attr
    {
        [Key("C")]
        public MpColor4? mColor;
    }

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

        static void test002()
        {
            Attr attr1 = new Attr();

            attr1.mColor = MpColor4.Create(Color4.LightGray);

            byte[] data = MessagePackSerializer.Serialize(attr1);

            Attr attr2 = MessagePackSerializer.Deserialize<Attr>(data);

            Console.WriteLine("");
        }

        static void Main(string[] args)
        {
            test002();
            Console.WriteLine("<<< END >>>");
            Console.ReadLine();
        }
    }
}
