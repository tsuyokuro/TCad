// 強制的にリソース文字列をUSにする
// ForceLinePen resource string to US
//#define FORCE_US

using Plotter;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using TCad.Plotter.Serializer;

namespace TCad;

public partial class App : Application
{
    private MySplashWindow SplashWindow = null;

    Stopwatch StartUpSW = new Stopwatch();

    private static bool NowExceptionHandling = false;

    public App()
    {
        //Log.LogOutput = new LogVisualStudioDebug();
        //Log.LogOutput = new LogConsole();
        Log.LogOutput = new LogDebugServer();

        StartUpSW.Start();

#if FORCE_US
        CultureInfo ci = new CultureInfo("en-US");
        Thread.CurrentThread.CurrentCulture = ci;
        Thread.CurrentThread.CurrentUICulture = ci;
#endif

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        SetupExceptionHandling();
    }

    [STAThread]
    override protected void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        Log.pl($"Total Memory = {GC.GetTotalMemory(true) / 1024} KB");

        // アニメーションのためSplashWindowを別Threadで表示
        Thread thread = new Thread(() =>
        {
            SplashWindow = new MySplashWindow();

            // Memory leakよけ 
            SplashWindow.Closed += (o, args) =>
            {
                Dispatcher.CurrentDispatcher.BeginInvokeShutdown(DispatcherPriority.Background);
            };


            SplashWindow.Show();

            Dispatcher.Run();
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();

        Thread.Sleep(500);
        Log.pl($"Total Memory = {GC.GetTotalMemory(true) / 1024} KB");

        Stopwatch sw = new Stopwatch();
        sw.Start();

        // MessagePack for C# は、初回の実行が遅いので、起動時にダミーを実行して
        // 紛れさせる
        MpInitializer.Init();


        MainWindow = new MainWindow();

        MainWindow.Show();

        sw.Stop();

        Log.pl($"MainWindow startup. Start up time: {sw.ElapsedMilliseconds} (milli sec)");

        Log.pl($"Total Memory = {GC.GetTotalMemory(true) / 1024} KB");

        SplashWindow.Dispatcher.Invoke(() =>
        {
            SplashWindow.Close();
            SplashWindow = null;
        });

        Thread.Sleep(50);
        GC.Collect();
        Log.pl($"Total Memory = {GC.GetTotalMemory(true) / 1024} KB");
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Log.Stop();
        base.OnExit(e);
    }

    // 例外ハンドリングの設定
    private void SetupExceptionHandling()
    {
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
        Current.Dispatcher.Invoke(() => { HandleException(e.Exception); });
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
}
