using System;
using System.Threading;

using TCad.WindowsAPI;

namespace TCad.Logger;


public interface ILogWriter
{
    void Start();
    void Stop();

    void Write(string s);
    void WriteLine(string s);
}


public class LogConsole : ILogWriter
{
    public void Start()
    {

        WinAPI.AllocConsole();
    }

    public void Stop()
    {
        WinAPI.FreeConsole();
    }


    public void Write(string s)
    {
        Console.Write(s);
    }

    public void WriteLine(string s)
    {
        Console.WriteLine(s);
    }
}

public class LogDebugServer : ILogWriter
{
    DebugServer DServer;

    public void Start()
    {
        if (DServer != null)
        {
            DServer.Start();
        }
    }
    public void Stop()
    {
        if (DServer != null)
        {
            DServer.Stop();
        }
    }

    public LogDebugServer()
    {
        DServer = new DebugServer();
    }

    public void Write(string s)
    {
        DServer.Write(s);
    }

    public void WriteLine(string s)
    {
        DServer.WriteLine(s);
    }
}

public class LogVisualStudioDebug : ILogWriter
{
    public void Start() { }
    public void Stop() { }


    public void Write(string s)
    {
        System.Diagnostics.Debug.Write(s);
    }

    public void WriteLine(string s)
    {
        System.Diagnostics.Debug.WriteLine(s);
    }
}


public static class Log
{
    public static ulong PutCount = 0;

    public static int mIndent = 0;
    public static int IndentUnit = 2;

    public static string space = "";

    private static ILogWriter LogWriter_;
    public static ILogWriter LogWriter
    {
        get => LogWriter_;
        set
        {
            if (LogWriter_ != null)
            {
                LogWriter_.Stop();
            }

            LogWriter_ = value;


            Print = LogWriter_.Write;
            PrintLn = LogWriter_.WriteLine;
            LogWriter_.Start();
        }
    }

    public static Action<string> Print = (s) => { /* NOP */ };
    public static Action<string> PrintLn = (s) => { /* NOP */ };

    public static Mutex Lock = new Mutex();


    public static void Start()
    {
        LogWriter!.Start();
    }

    public static void Stop()
    {
        LogWriter?.Stop();
    }


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
