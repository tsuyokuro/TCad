using GLFont;
using GLUtil;
using OpenGL.GLU;
using Plotter;
using Plotter.Controller;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using TCad.Dialogs;
using TCad.Util;
using TCad.ViewModel;

namespace TCad;

public partial class MainWindow : Window, ICadMainWindow
{
    public IPlotterViewModel ViewModel;

    private ImageSource[] PopupMessageIcons = new ImageSource[3];

    public MainWindow()
    {
        Log.plx("in");

        InitializeComponent();

        Glu.Initialize();

        ViewModel = new PlotterViewModel(this);

        ViewModel.Open();

        ViewModel.ObjectTree = ObjTree;

        ViewModel.AttachCommandView(textCommand);

        SetupInteractionConsole();

        KeyDown += OnKeyDown;
        KeyUp += OnKeyUp;

        Loaded += MainWindow_Loaded;
        Closed += MainWindow_Closed;

        SetupDataContext();

        InitWindowChrome();

        InitPopup();

        Log.plx("out");
    }

    private void SetupInteractionConsole()
    {
        ItConsole.Print = MyConsole.Print;
        ItConsole.PrintLn = MyConsole.PrintLn;
        ItConsole.PrintF = MyConsole.PrintF;
        ItConsole.Clear = MyConsole.Clear;
        ItConsole.GetString = PromptTextInput;
    }

    private string PromptTextInput(string msg, string def)
    {
        InputStringDialog dlg = new InputStringDialog();

        dlg.Message = msg;

        dlg.InputString = def;

        bool? dlgRet = dlg.ShowDialog();

        if (!dlgRet.Value)
        {
            return null;
        }

        return dlg.InputString;
    }

    private void SetupDataContext()
    {
        LayerListView.DataContext = ViewModel.LayerListVM;

        SelectModePanel.DataContext = ViewModel;
        FigurePanel.DataContext = ViewModel;

        textBlockXYZ.DataContext = ViewModel.CursorPosVM;
        textBlockXYZ2.DataContext = ViewModel.CursorPosVM;
        textBlockXYZ3.DataContext = ViewModel.CursorPosVM;

        MainMenu.DataContext = ViewModel;

        FileToolBar.DataContext = ViewModel;

        CommandBar.DataContext = ViewModel;

        FileName.DataContext = ViewModel;

        ViewModePanel.DataContext = ViewModel.ViewManager;
        ResetCameraButton.DataContext = ViewModel;

        SnapToolBar.DataContext = ViewModel.Settings;

        LayerToolBar.DataContext = ViewModel;

        TreeViewToolBar.DataContext = ViewModel;
        TreeViewToolBar_FilterButton.DataContext = ViewModel.Settings;
    }

    private void InitWindowChrome()
    {
        BtnCloseWindow.Click += (sender, e) => { Close(); };
        BtnMinWindow.Click += (sender, e) => { WindowState = WindowState.Minimized; };
        BtnMaxWindow.Click += (sender, e) => { WindowState = WindowState.Maximized; };
        BtnRestWindow.Click += (sender, e) => { WindowState = WindowState.Normal; };

        StateChanged += MainWindow_StateChanged;
    }

    private void InitPopup()
    {
        InitPopupMessageIcons();
        PopupMessage.CustomPopupPlacementCallback = PlaceMessagePopup;
    }

    private void InitPopupMessageIcons()
    {
        PopupMessageIcons[(int)UITypes.MessageType.INFO] =
            (ImageSource)TryFindResource("infoIconDrawingImage");

        PopupMessageIcons[(int)UITypes.MessageType.INPUT] =
            (ImageSource)TryFindResource("inputIconDrawingImage");

        PopupMessageIcons[(int)UITypes.MessageType.ERROR] =
            (ImageSource)TryFindResource("errorIconDrawingImage");
    }

    public CustomPopupPlacement[] PlaceMessagePopup(Size popupSize,
                                       Size targetSize,
                                       Point offset)
    {
        double rightOffset = 2.0;
        double topOffset = 2.0;

        Point p = new Point(targetSize.Width - popupSize.Width - rightOffset, topOffset);

        CustomPopupPlacement placement1 =
            new CustomPopupPlacement(p, PopupPrimaryAxis.Horizontal);

        CustomPopupPlacement[] ttplaces =
                new CustomPopupPlacement[] { placement1 };
        return ttplaces;
    }

    private ImageSource SelectPopupMessageIcon(UITypes.MessageType type)
    {
        return PopupMessageIcons[(int)type];
    }

    private void MainWindow_StateChanged(object sender, EventArgs e)
    {
        switch (WindowState)
        {
            case WindowState.Maximized:
                Task.Run(() =>
                {
                    Thread.Sleep(10);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        LayoutRoot.Margin = new Thickness(-1);

                    });
                });

                break;
            default:
                LayoutRoot.Margin = new Thickness(0);
                break;
        }
    }

    private void MainWindow_Closed(object sender, EventArgs e)
    {
        ViewModel.Close();
        ImageRenderer.Provider.Release();
        FontRenderer.Instance.Dispose();
        TextureProvider.Instance.RemoveAll();
        FontShader.GetInstance().Dispose();
        ImageShader.GetInstance().Dispose();
        WireFrameShader.GetInstance().Dispose();

        Glu.Dispose();
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        Log.plx("in");

        //var hsrc = HwndSource.FromVisual(this) as HwndSource;
        //hsrc.AddHook(WndProc);

        ColorPack cp = ViewModel.DC.Tools.Brush(DrawTools.BRUSH_BACKGROUND).ColorPack;
        XamlResource.SetValue("MainViewHostBGColor", new SolidColorBrush(Color.FromRgb(cp.R, cp.G, cp.B)));

        ImageRenderer.Provider.Get();

        WireFrameShader.GetInstance();

        Log.plx("out");
    }

    #region "Key handling"
    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (!textCommand.IsFocused && !MyConsole.IsFocused)
        {
            e.Handled = ViewModel.OnKeyDown(sender, e);
        }
    }

    private void OnKeyUp(object sender, KeyEventArgs e)
    {
        if (!textCommand.IsFocused && !MyConsole.IsFocused)
        {
            e.Handled = ViewModel.OnKeyUp(sender, e);
        }
        else
        {
            if (e.Key == Key.Escape)
            {
                viewContainer.Focus();
            }
        }
    }
    #endregion


    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        var handle = new WindowInteropHelper(this).Handle;
        if (handle == IntPtr.Zero)
        {
            return;
        }
        HwndSource.FromHwnd(handle).AddHook(this.WndProc);
    }

    IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        switch (msg)
        {
            case WinAPI.WM_ENTERSIZEMOVE:
                {
                    MainWindow wnd = (MainWindow)Application.Current.MainWindow;
                    wnd.viewContainer.Visibility = Visibility.Hidden;
                }
                break;

            case WinAPI.WM_EXITSIZEMOVE:
                {
                    MainWindow wnd = (MainWindow)Application.Current.MainWindow;
                    if (wnd.viewContainer.Visibility != Visibility.Visible)
                    {
                        wnd.viewContainer.Visibility = Visibility.Visible;
                        ViewModel.Redraw();
                    }
                }
                break;

            case WinAPI.WM_MOVE:
                {
                    // キャプションの上端でResizeすると画面崩れするのでコメントアウト
                    //MainWindow wnd = (MainWindow)Application.Current.MainWindow;
                    //if (wnd.viewContainer.Visibility != Visibility.Visible)
                    //{
                    //    wnd.viewContainer.Visibility = Visibility.Visible;
                    //}
                }
                break;

            case WinAPI.WM_SIZE:
                break;
            case WinAPI.WM_GETMINMAXINFO:
                {
                    var result = OnGetMinMaxInfo(hwnd, wParam, lParam);
                    if (result != null)
                    {
                        handled = true;
                        return result.Value;
                    }
                }
                break;
        }
        return IntPtr.Zero;
    }

    private IntPtr? OnGetMinMaxInfo(IntPtr hwnd, IntPtr wParam, IntPtr lParam)
    {
        var monitor = WinAPI.Monitor.MonitorFromWindow(hwnd, WinAPI.Monitor.MONITOR_DEFAULTTONEAREST);
        if (monitor == IntPtr.Zero)
        {
            return null;
        }

        var monitorInfo = new WinAPI.Monitor.MONITORINFO();
        monitorInfo.Size = Marshal.SizeOf(monitorInfo);
        if (WinAPI.Monitor.GetMonitorInfo(monitor, ref monitorInfo) != true)
        {
            return null;
        }

        var workingRectangle = monitorInfo.WorkRect;
        var monitorRectangle = monitorInfo.MonitorRect;
        var minmax = (WinAPI.MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(WinAPI.MINMAXINFO));
        minmax.MaxPosition.X = Math.Abs(workingRectangle.Left - monitorRectangle.Left);
        minmax.MaxPosition.Y = Math.Abs(workingRectangle.Top - monitorRectangle.Top);
        minmax.MaxSize.X = Math.Abs(workingRectangle.Right - monitorRectangle.Left);
        minmax.MaxSize.Y = Math.Abs(workingRectangle.Bottom - monitorRectangle.Top);
        Marshal.StructureToPtr(minmax, lParam, true);
        return IntPtr.Zero;
    }


    #region IMainWindow
    public void SetPlotterView(IPlotterView view)
    {
        viewContainer.Child = view.FormsControl;
    }

    public void OpenPopupMessage(string text, UITypes.MessageType messageType)
    {
        ThreadUtil.RunOnMainThread(() => {
            if (PopupMessage.IsOpen) return;

            PopupMessageIcon.Source = SelectPopupMessageIcon(messageType);
            PopupMessageText.Text = text;
            PopupMessage.IsOpen = true;
        }, true);
    }

    public void ClosePopupMessage()
    {
        ThreadUtil.RunOnMainThread(() =>
        {
            PopupMessage.IsOpen = false;
        }, true);
    }
    #endregion

    #region ViewManager Event
    public void DrawModeChanged(DrawModes drawMode)
    {
        Log.plx("in");
        ColorPack cp = ViewModel.DC.Tools.Brush(DrawTools.BRUSH_BACKGROUND).ColorPack;
        XamlResource.SetValue("MainViewHostBGColor", new SolidColorBrush(Color.FromRgb(cp.R, cp.G, cp.B)));

        //XamlResource.SetValue("BaseColor", new SolidColorBrush(Colors.White));
        //ObjTree.Background = Brushes.Beige;
        //ObjTree.NodeFG = Brushes.Black;
        //ObjTree.LeafFG = Brushes.DarkCyan;
        //ObjTree.Redraw();
        Log.plx("out");
    }
    #endregion
}
