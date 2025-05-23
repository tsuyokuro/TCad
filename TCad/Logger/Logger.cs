using System;
using System.Diagnostics;
using System.Text;
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
        Debug.Write(s);
    }

    public void WriteLine(string s)
    {
        Debug.WriteLine(s);
    }
}

public class StringWriter : ILogWriter
{
    private StringBuilder sb = new StringBuilder();

    public void Start() { }
    public void Stop() { }


    public void Write(string s)
    {
        sb.Append(s);
    }

    public void WriteLine(string s)
    {
        sb.Append(s);
        sb.Append("\n");
    }

    public string GetString()
    {
        return sb.ToString();
    }

    public void Clear()
    {
        sb.Clear();
    }
}

public class NopLogWriter : ILogWriter
{
    public void Start() { }
    public void Stop() { }
    public void Write(string s) { }
    public void WriteLine(string s){ }
}

public interface ILogFormatter
{
    int Indent { get; set; }
    ILogWriter LogWriter { get; set; }

    void p(string s);
    void pl(string s);
    void plx(string s);
    void ResetIndent();
    void tpl(string s);
}

public class LogFormatter : ILogFormatter
{
    public int IndentChars = 2;

    private string space = "";

    public ILogWriter LogWriter
    {
        get;
        set;
    }

    private int Indent_ = 0;
    public int Indent
    {
        set
        {
            Indent_ = value;
            space = new string(' ', Indent_ * IndentChars);
        }

        get => Indent_;
    }

    private int UpStackFrame = 1;

    public LogFormatter(ILogWriter writer, int upStackFrame = 1)
    {
        LogWriter = writer;
        UpStackFrame = upStackFrame;
    }

    public LogFormatter(int upStackFrame = 1)
    {
        LogWriter = new NopLogWriter();
        UpStackFrame = upStackFrame;
    }

    public void ResetIndent()
    {
        Indent = 0;
        IndentChars = 2;
        space = "";
    }

    public void p(string s)
    {
        LogWriter.Write(s);
    }

    public void pl(string s)
    {
        LogWriter.WriteLine(space + s);
    }

    public void tpl(string s)
    {
        DateTime dt = DateTime.Now;

        int tid = Thread.CurrentThread.ManagedThreadId;

        LogWriter.WriteLine(dt.ToString("HH:mm:ss.fff") + " " + tid + " " + space + s);
    }

    public void plx(string s)
    {
        StackFrame stackFrame = new StackFrame(UpStackFrame);

        string method = stackFrame.GetMethod().Name;
        string klass = stackFrame.GetMethod().ReflectedType.Name;

        DateTime dt = DateTime.Now;
        int tid = Thread.CurrentThread.ManagedThreadId;

        LogWriter.WriteLine(
            dt.ToString("HH:mm:ss.fff") + " " + tid + " " +
            space + klass + "," + method + " " + s
            );
    }
}

public static class Log
{
    private static Mutex Lock = new Mutex();

    private static ILogFormatter Formatter = new LogFormatter(upStackFrame: 2);

    public static int Indent
    {
        get => Formatter.Indent;
        set => Formatter.Indent = value;
    }

    private static ILogWriter LogWriter_;
    public static ILogWriter LogWriter
    {
        get => LogWriter_;
        set
        {
            LogWriter_?.Stop();
            LogWriter_ = value;

            LogWriter_ ??= new NopLogWriter();

            Formatter.LogWriter = LogWriter_;

            LogWriter_.Start();
        }
    }

    public static int UpStackFrame = 1;


    public static void Start()
    {
    }

    public static void Stop()
    {
        LogWriter?.Stop();
        LogWriter = null;
    }

    public static void ResetIndent()
    {
        Lock.WaitOne();
        Formatter.ResetIndent();
        Lock.ReleaseMutex();
    }

    public static void p(string s)
    {
        Lock.WaitOne();
        Formatter.p(s);
        Lock.ReleaseMutex();
    }

    public static void pl(string s)
    {
        Lock.WaitOne();
        Formatter.pl(s);
        Lock.ReleaseMutex();
    }

    public static void tpl(string s)
    {
        Lock.WaitOne();
        Formatter.tpl(s);
        Lock.ReleaseMutex();
    }

    public static void plx(string s)
    {
        Lock.WaitOne();
        Formatter.plx(s);
        Lock.ReleaseMutex();
    }
}
