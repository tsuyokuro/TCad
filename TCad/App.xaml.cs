
/**
 * TODO Consoleの利用について
 * 
 * プロジェクト->TCadのプロパティー->アプリケーソンタブ
 * 出力の種類をコンソールアプリケーションにするとデバッグ実行時も
 * コンソールに出力されるようになる
 * コンソールの使用を止めるときは、出力の種類を Windowsアプリケーションもどすこと
 *
 * 
 * Visual studio 2017 15.8.4では、Windowsアプリケーションのまま、普通にコンソールに出力される
 * Visual studio 2017 15.9.4では、またこの手順が必要になった
 **/
#define USE_CONSOLE
//#define USE_CONSOL_INPUT

// 強制的にリソース文字列をUSにする
// Force resource string to US
//#define FORCE_US

using TCad.Util;
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

namespace TCad
{
    public partial class App : Application
    {
#if USE_CONSOLE
        public const bool UseConsole = true;
#else
        public const bool UseConsole = false;
#endif
        private MySplashWindow SplashWindow = null;

        private TaskScheduler mMainThreadScheduler;

        private DebugInputThread InputThread = null;

        private DebugClient DClient;

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

#if USE_CONSOLE
            WinAPI.AllocConsole();
            Console.WriteLine("App OnStartup USE_CONSOLE");
#endif

#if USE_CONSOL_INPUT
            InputThread = new DebugInputThread();
            InputThread.start();
#endif

            mMainThreadScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            ThreadUtil.Init();

            OpenTK.Toolkit.Init();

            // MessagePack for C# は、初回の実行が遅いので、起動時にダミーを実行して
            // 紛れさせる
            MpInitializer.Init();

            SetupDebugConsole();

            MainWindow = new MainWindow();

            MainWindow.Show();

            sw.Stop();

            Console.WriteLine($"MainWindow startup. Start up time: {sw.ElapsedMilliseconds} (milli sec)");

            SplashWindow.Close();
            SplashWindow = null;
        }

        private void SetupDebugConsole()
        {
            if (UseConsole)
            {
                DOut.PrintF = Console.Write;
                DOut.PrintLn = Console.WriteLine;

                DOut.pl("DOut's output setting is Console");

                //DClient = new DebugClient();

                ////if (DClient.IsValid)
                //{
                //    DOut.PrintFunc = DClient.Write;
                //    DOut.PrintLnFunc = DClient.WriteLine;

                //    DOut.pl("DOut's output setting is DebugServer");
                //}
            }
            else
            {
                //MainWindow wnd = (MainWindow)MainWindow;
                //DOut.PrintF = wnd.GetBuiltinConsole().Print;
                //DOut.PrintLn = wnd.GetBuiltinConsole().PrintLn;
            }
        }


        // e.g. ReadResourceText("/Shader/font_fragment.shader")
        public static string ReadResourceText(string path)
        {
            Uri fileUri = new Uri(path, UriKind.Relative);
            StreamResourceInfo info = Application.GetResourceStream(fileUri);
            StreamReader sr = new StreamReader(info.Stream);

            string s = sr.ReadToEnd();
            sr.Close();

            return s;
        }

        protected override void OnExit(ExitEventArgs e)
        {
#if USE_CONSOLE
            WinAPI.FreeConsole();
#endif
            base.OnExit(e);
        }
    }
}
