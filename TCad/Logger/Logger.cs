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
    private readonly DebugServer DServer;

    public void Start()
    {
        DServer?.Start();
    }
    public void Stop()
    {
        DServer?.Stop();
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
    private readonly StringBuilder sb = new();

    public void Start() { }
    public void Stop() { }


    public void Write(string s)
    {
        sb.Append(s);
    }

    public void WriteLine(string s)
    {
        sb.Append(s);
        sb.Append('\n');
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

public class LogFormatter
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

    private readonly int UpStackFrame = 1;

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

        int tid = Environment.CurrentManagedThreadId;

        LogWriter.WriteLine(dt.ToString("HH:mm:ss.fff") + " " + tid + " " + space + s);
    }

    public void plx(string s)
    {
        StackFrame stackFrame = new(UpStackFrame);

        string method = stackFrame.GetMethod().Name;
        string klass = stackFrame.GetMethod().ReflectedType.Name;

        DateTime dt = DateTime.Now;
        int tid = Environment.CurrentManagedThreadId;

        LogWriter.WriteLine(
            dt.ToString("HH:mm:ss.fff") + " " + tid + " " +
            space + klass + "," + method + " " + s
            );
    }
}

public interface ILogDelegate
{
    int Indent { get; set; }
    ILogWriter LogWriter { get; set; }

    void p(string s);
    void pl(string s);
    void plx(string s);
    void ResetIndent();
    void Start();
    void Stop();
    void tpl(string s);
}

public class LogDelegate : ILogDelegate
{
    private readonly Mutex Lock = new();

    private readonly LogFormatter Formatter = new(upStackFrame: 3);

    public int Indent
    {
        get => Formatter.Indent;
        set => Formatter.Indent = value;
    }

    private ILogWriter LogWriter_;
    public ILogWriter LogWriter
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

    public void Start()
    {
    }

    public void Stop()
    {
        LogWriter?.Stop();
        LogWriter = null;
    }

    public void ResetIndent()
    {
        Lock.WaitOne();
        Formatter.ResetIndent();
        Lock.ReleaseMutex();
    }

    public void p(string s)
    {
        Lock.WaitOne();
        Formatter.p(s);
        Lock.ReleaseMutex();
    }

    public void pl(string s)
    {
        Lock.WaitOne();
        Formatter.pl(s);
        Lock.ReleaseMutex();
    }

    public void tpl(string s)
    {
        Lock.WaitOne();
        Formatter.tpl(s);
        Lock.ReleaseMutex();
    }

    public void plx(string s)
    {
        Lock.WaitOne();
        Formatter.plx(s);
        Lock.ReleaseMutex();
    }
}


public static class Log
{
    private static ILogDelegate LogDelegate_ = new LogDelegate();

    public static int Indent
    {
        get => LogDelegate_.Indent;
        set => LogDelegate_.Indent = value;
    }

    public static ILogWriter LogWriter
    {
        get => LogDelegate_.LogWriter;
        set => LogDelegate_.LogWriter = value;
    }

    public static void Start()
    {
        LogDelegate_.Start();
    }

    public static void Stop()
    {
        LogDelegate_.Stop();
    }

    public static void ResetIndent()
    {
        LogDelegate_.ResetIndent();
    }

    public static void p(string s)
    {
        LogDelegate_.p(s);
    }

    public static void pl(string s)
    {
        LogDelegate_.pl(s);
    }

    public static void tpl(string s)
    {
        LogDelegate_.tpl(s);
    }

    public static void plx(string s)
    {
        LogDelegate_.plx(s);
    }
}
