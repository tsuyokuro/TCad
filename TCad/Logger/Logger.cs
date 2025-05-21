using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
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
        System.Diagnostics.Debug.Write(s);
    }

    public void WriteLine(string s)
    {
        System.Diagnostics.Debug.WriteLine(s);
    }
}

public class StringWriter : ILogWriter
{
    StringBuilder sb = new StringBuilder();

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


public interface ILogPrinter
{
    int Indent { get; set; }
    ILogWriter LogWriter { get; set; }

    void Start();

    void Stop();

    void Begin();

    void End();

    void Reset();

    void p(string s);

    void pl(string s);

    void plx(string s);

    void tpl(string s);
}

public class LogPrinter : ILogPrinter
{
    public ulong PutCount = 0;

    private int mIndent = 0;

    public int IndentChars = 2;

    private string space = "";

    private ILogWriter LogWriter_;
    public ILogWriter LogWriter
    {
        get => LogWriter_;
        set
        {
            LogWriter_?.Stop();    
            LogWriter_ = value;

            if (LogWriter_ == null)
            {
                LogWriter_ = new NopLogWriter();
            }

            LogWriter_.Start();
        }
    }

    public int UpStackFrame = 1;

    public LogPrinter(ILogWriter writer, int upStackFrame = 1)
    {
        LogWriter = writer;
        UpStackFrame = upStackFrame;
    }

    public LogPrinter(int upStackFrame = 1)
    {
        LogWriter = new NopLogWriter();
        UpStackFrame = upStackFrame;
    }

    public Mutex Lock = new Mutex();


    private void Print(string s)
    {
        LogWriter?.Write(s);
    }

    private void PrintLn(string s)
    {
        LogWriter?.WriteLine(s);
    }

    public void Start()
    {
        LogWriter?.Start();
    }

    public void Stop()
    {
        LogWriter?.Stop();
    }


    public int Indent
    {
        set
        {
            mIndent = value;
            space = new string(' ', mIndent * IndentChars);
        }

        get => mIndent;
    }

    public void Reset()
    {
        Begin();
        mIndent = 0;
        IndentChars = 2;
        space = "";
        End();
    }

    public void Begin()
    {
        Lock.WaitOne();
    }

    public void End()
    {
        Lock.ReleaseMutex();
    }

    // Print without new line
    public void p(string s)
    {
        Begin();
        PutCount++;
        Print(s);
        End();
    }

    // Print with new line
    public void pl(string s)
    {
        Begin();
        PutCount++;
        PrintLn(space + s);
        End();
    }

    // Print with new line
    public void tpl(string s)
    {
        DateTime dt = DateTime.Now;

        int tid = Thread.CurrentThread.ManagedThreadId;

        Begin();
        PutCount++;
        PrintLn(dt.ToString("HH:mm:ss.fff") + " " + tid + " " + space + s);
        End();
    }

    public void plx(string s)
    {
        StackFrame stackFrame = new StackFrame(UpStackFrame);

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


public static class Log
{
    private static ILogPrinter LogPrinter_ = new LogPrinter(2);

    public static ILogWriter LogWriter
    {
        get => LogPrinter_.LogWriter;

        set
        {
            LogPrinter_.LogWriter = value;
        }
    }

    public static int Indent
    {
        set => LogPrinter_.Indent = value;
        get => LogPrinter_.Indent;
    }
    public static Action Start => LogPrinter_.Start;
    public static Action Stop => LogPrinter_.Stop;
    public static Action reset => LogPrinter_.Reset;
    public static Action Begin => LogPrinter_.Begin;
    public static Action End => LogPrinter_.End;
    public static Action<String> p => LogPrinter_.p;
    public static Action<String> pl => LogPrinter_.pl;
    public static Action<String> tpl => LogPrinter_.tpl;
    public static Action<String> plx => LogPrinter_.plx;
}
