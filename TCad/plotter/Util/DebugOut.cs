using System;
using System.Threading;

namespace Plotter
{
    public static class DOut
    {
        public static ulong PutCount = 0;

        public static int mIndent = 0;
        public static int IndentUnit = 2;

        public static String space = "";

        public static Action<string> PrintF = (s)=> { };
        public static Action<string> PrintLn = (s)=> { };

        public static Mutex Lock = new Mutex();

        public static int Indent
        {
            set
            {
                mIndent = value;
                space = new string(' ', mIndent * IndentUnit);
            }

            get
            {
                return mIndent;
            }
        }

        public static void reset()
        {
            Begin();
            mIndent = 0;
            IndentUnit = 2;
            space = "";
            End();
        }

        public static void Begin()
        {
            Lock.WaitOne();
        }

        public static void End()
        {
            Lock.ReleaseMutex();
        }

        public static void printIndent()
        {
            p(space);
        }

        // print without new line
        public static void p(String s)
        {
            Begin();
            PutCount++;
            PrintF(s);
            End();
        }

        // print with new line
        public static void pl(String s)
        {
            Begin();
            PutCount++;
            PrintLn(space + s);
            End();
        }

        // print with new line
        public static void tpl(String s)
        {
            DateTime dt = DateTime.Now;

            Begin();
            PutCount++;
            PrintLn(dt.ToString("HH:mm:ss.fff") + " " + space + s);
            End();
        }
    }
}
