//#define DEFAULT_DATA_TYPE_DOUBLE
using System;
using System.Threading;



#if DEFAULT_DATA_TYPE_DOUBLE
using vcompo_t = System.Double;
using vector3_t = OpenTK.Mathematics.Vector3d;
using vector4_t = OpenTK.Mathematics.Vector4d;
using matrix4_t = OpenTK.Mathematics.Matrix4d;
#else
using vcompo_t = System.Single;
using vector3_t = OpenTK.Mathematics.Vector3;
using vector4_t = OpenTK.Mathematics.Vector4;
using matrix4_t = OpenTK.Mathematics.Matrix4;
#endif


namespace Plotter;

public static class Log
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
