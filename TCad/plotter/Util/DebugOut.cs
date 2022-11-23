using System;
using System.Threading;

namespace Plotter;

public static class DOut
{
    public static ulong PutCount = 0;

    public static int mIndent = 0;
    public static int IndentUnit = 2;

    public static string space = "";

    public static Action<string> Print = (s)=> { };
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

    // Print without new line
    public static void p(string s)
    {
        Begin();
        PutCount++;
        Print(s);
        End();
    }

    // Print with new line
    public static void pl(string s)
    {
        Begin();
        PutCount++;
        PrintLn(space + s);
        End();
    }

    // Print with new line
    public static void tpl(string s)
    {
        DateTime dt = DateTime.Now;

        int tid = Thread.CurrentThread.ManagedThreadId;

        Begin();
        PutCount++;
        PrintLn(dt.ToString("HH:mm:ss.fff") + " " + tid + " " + space + s);
        End();
    }

    public static void plx(string s)
    {
        System.Diagnostics.StackFrame stackFrame = new System.Diagnostics.StackFrame(1);

        string method = stackFrame.GetMethod().Name;
        string klass = stackFrame.GetMethod().ReflectedType.Name;

        DateTime dt = DateTime.Now;
        int tid = Thread.CurrentThread.ManagedThreadId;

        Begin();
        PutCount++;
        PrintLn(dt.ToString("HH:mm:ss.fff") + " " + tid + " " +
            space + klass + "," + method + " " + s);
        End();
    }
}
