// 強制的にリソース文字列をUSにする
// ForceLinePen resource string to US
//#define FORCE_US

using Plotter;
using Plotter.Serializer;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Resources;
using System.Windows.Threading;

namespace TCad;

public partial class App : Application
{
    private MySplashWindow SplashWindow = null;

    private TaskScheduler mMainThreadScheduler;

    Stopwatch StartUpSW = new Stopwatch();


    public static App GetCurrent()
    {
        return (App)Current;
    }

    public static TaskScheduler MainThreadScheduler
    {
        get
        {
            return GetCurrent().mMainThreadScheduler;
        }
    }

    private static bool NowExceptionHandling = false;

    public App()
    {
        StartUpSW.Start();
        //Log.LogOutput = new LogVisualStudioDebug();
        //Log.LogOutput = new LogConsole();
        Log.LogOutput = new LogDebugServer();

#if FORCE_US
        CultureInfo ci = new CultureInfo("en-US");
        Thread.CurrentThread.CurrentCulture = ci;
        Thread.CurrentThread.CurrentUICulture = ci;
#endif

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        DispatcherUnhandledException += App_DispatcherUnhandledException;
        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

        AppDomain currentDomain = AppDomain.CurrentDomain;
        currentDomain.UnhandledException += CurrentDomain_UnhandledException;

        System.Windows.Forms.Application.ThreadException += Application_ThreadException;
    }
         
    private void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
    {
        HandleException(e.Exception);
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        HandleException(e.ExceptionObject);
    }

    // UI ThreadのException
    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        e.Handled = true;
        HandleException(e.Exception);
    }

    // 別ThreadのException
    private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
    {
        new Task(() =>
        {
            HandleException(e.Exception);
        }
        ).Start(mMainThreadScheduler);
    }

    public static void ThrowException(object e)
    {
        new Task(() =>
        {
            GetCurrent().HandleException(e);
        }
        ).Start(GetCurrent().mMainThreadScheduler);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private void HandleException(object e)
    {
        if (NowExceptionHandling)
        {
            return;
        }

        NowExceptionHandling = true;

        if (!ShowExceptionDialg(e.ToString()))
        {
            Shutdown();
        }

        NowExceptionHandling = false;
    }

    public static bool ShowExceptionDialg(string text)
    {
        EceptionDialog dlg = new EceptionDialog();

        dlg.text.Text = text;
        bool? result = dlg.ShowDialog();

        if (result == null) return false;

        return result.Value;
    }

    [STAThread]
    override protected void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        SplashWindow = new MySplashWindow();
        SplashWindow.Show();

        Stopwatch sw = new Stopwatch();
        sw.Start();


        mMainThreadScheduler = TaskScheduler.FromCurrentSynchronizationContext();

        // MessagePack for C# は、初回の実行が遅いので、起動時にダミーを実行して
        // 紛れさせる
        MpInitializer.Init();


        MainWindow = new MainWindow();

        MainWindow.Show();

        sw.Stop();

        Log.pl($"MainWindow startup. Start up time: {sw.ElapsedMilliseconds} (milli sec)");

        SplashWindow.Close();
        SplashWindow = null;
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Log.Stop();
        base.OnExit(e);
    }
}
