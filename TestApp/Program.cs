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

namespace TestApp
{
    public class AnsiEsc
    {
        public const string ESC = "\x1b[";

        public const string Reset = ESC + "0m";

        public const string Black = ESC + "30m";
        public const string Red = ESC + "31m";
        public const string Green = ESC + "32m";
        public const string Yellow = ESC + "33m";
        public const string Blue = ESC + "34m";
        public const string Magenta = ESC + "35m";
        public const string Cyan = ESC + "36m";
        public const string White = ESC + "37m";

        public const string BBlack = ESC + "90m";
        public const string BRed = ESC + "91m";
        public const string BGreen = ESC + "92m";
        public const string BYellow = ESC + "93m";
        public const string BBlue = ESC + "94m";
        public const string BMagenta = ESC + "95m";
        public const string BCyan = ESC + "96m";
        public const string BWhite = ESC + "97m";


        public const string BlackBG = ESC + "40m";
        public const string RedBG = ESC + "41m";
        public const string GreenBG = ESC + "42m";
        public const string YellowBG = ESC + "43m";
        public const string BlueBG = ESC + "44m";
        public const string MagentaBG = ESC + "45m";
        public const string CyanBG = ESC + "46m";
        public const string WhiteBG = ESC + "47m";

        public const string BBlackBG = ESC + "100m";
        public const string BRedBG = ESC + "101m";
        public const string BGreenBG = ESC + "102m";
        public const string BYellowBG = ESC + "103m";
        public const string BBlueBG = ESC + "104m";
        public const string BMagentaBG = ESC + "105m";
        public const string BCyanBG = ESC + "106m";
        public const string BWhiteBG = ESC + "107m";
    }

    public struct TextAttr
    {
        public int FColor;
        public int BColor;
    }

    public struct AttrSpan
    {
        public TextAttr Attr;
        public int Start;
        public int Len;

        public AttrSpan(TextAttr attr, int start, int len)
        {
            Attr = attr;
            Start = start;
            Len = len;
        }
    }

    public class TextLine
    {
        public string Data = "";
        public List<AttrSpan> Attrs = new List<AttrSpan>();

        private AttrSpan LastAttrSpan
        {
            get
            {
                return Attrs[Attrs.Count - 1];
            }

            set
            {
                Attrs[Attrs.Count - 1] = value;
            }
        }

        public TextLine(TextAttr attr)
        {
            Attrs.Add(new AttrSpan(attr, 0, 0));
        }

        public void Clear()
        {
            TextAttr lastAttr = LastAttrSpan.Attr;
            Attrs.Clear();
            AppendAttr(lastAttr);
            Data = "";
        }

        public void AppendAttr(TextAttr attr)
        {
            AttrSpan lastSpan = LastAttrSpan;

            int sp = lastSpan.Start + lastSpan.Len;
            Attrs.Add(new AttrSpan(attr, sp, 0));
        }

        private void AddLastAttrSpanLen(int len)
        {
            AttrSpan attrItem = LastAttrSpan;
            attrItem.Len += len;
            LastAttrSpan = attrItem;
        }

        public void Parse(string str)
        {
            TextAttr attr = LastAttrSpan.Attr;

            StringBuilder sb = new StringBuilder();

            int blen = 0;

            int state = 0;

            int x = 0;

            ReadOnlySpan<char> s = str;

            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == '\x1b')
                {
                    state = 1;

                    AddLastAttrSpanLen(blen);

                    blen = 0;
                    continue;
                }

                switch (state)
                {
                    case 0:
                        if (s[i] == '\r')
                        {
                            Clear();
                            blen = 0;
                            sb.Clear();
                        }
                        else
                        {
                            sb.Append(s[i]);
                            blen++;
                        }
                        break;
                    case 1:
                        if (s[i] == '[')
                        {
                            state = 2;
                        }
                        break;
                    case 2:
                        if (s[i] >= '0' && s[i] <= '9')
                        {
                            state = 3;
                            x = s[i] - '0';
                        }
                        else if (s[i] == 'm')
                        {
                            if (x == 0)
                            {
                                attr.BColor = 0;
                                attr.FColor = 7;
                            }

                            AppendAttr(attr);

                            blen = 0;
                            state = 0;
                        }
                        else
                        {
                            sb.Append(s[i]);
                            blen++;
                            state = 0;
                        }
                        break;
                    case 3:
                        if (s[i] >= '0' && s[i] <= '9')
                        {
                            x = x * 10 + (s[i] - '0');
                        }
                        else if (s[i] == 'm')
                        {
                            if (x == 0)
                            {
                                attr.BColor = 0;
                                attr.FColor = 7;
                            }
                            else if (x >= 30 && x <= 37) // front std
                            {
                                attr.FColor = (byte)(x - 30);
                            }
                            else if (x >= 40 && x <= 47) // back std
                            {
                                attr.BColor = (byte)(x - 40);
                            }
                            else if (x >= 90 && x <= 97) // front strong
                            {
                                attr.FColor = (byte)(x - 90 + 8);
                            }
                            else if (x >= 100 && x <= 107) // back std
                            {
                                attr.BColor = (byte)(x - 100 + 8);
                            }

                            AppendAttr(attr);
                            blen = 0;
                            state = 0;
                        }
                        else
                        {
                            sb.Append(s[i]);
                            blen++;
                            state = 0;
                        }

                        break;
                }
            }

            if (blen > 0)
            {
                AddLastAttrSpanLen(blen);
                blen = 0;
            }

            Data += sb.ToString();
        }
    }

    internal class Program
    {
        static void test001()
        {
            TextAttr attr = default;
            attr.FColor = 1;
            attr.BColor = 0;

            TextLine tl = new TextLine(attr);

            string s = "漢字" + AnsiEsc.Green + "テスト" + AnsiEsc.Blue + AnsiEsc.GreenBG + "ABCdef";

            tl.Parse(s);

            var sw = new Stopwatch();
            sw.Start();

            ReadOnlySpan<char> tspan = tl.Data;
            StringBuilder sb = new StringBuilder(1024);

            for (int i=0; i<10000; i++)
            {
                foreach (AttrSpan span in tl.Attrs)
                {
                    if (span.Len == 0) continue;
                    ReadOnlySpan<char> pspan = tspan.Slice(span.Start, span.Len);

                    sb.Clear();
                    sb.Append(pspan);
                    string ps = sb.ToString();  

                    //Console.WriteLine(ps);
                }
            }

            sw.Stop();

            Console.WriteLine("" + sw.ElapsedMilliseconds);
        }

        static void Main(string[] args)
        {
            test001();
            Console.WriteLine("<<< END >>>");
            Console.ReadLine();
        }
    }
}
