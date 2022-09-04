using TCad.Util;
using Plotter;
using Plotter.Controller;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using TCad.Controls;
using TCad.ViewModel;
using TCad.Dialogs;

namespace TCad
{
    public partial class MainWindow : Window, ICadMainWindow
    {
        public PlotterViewModel ViewModel;

        private ImageSource[] PopupMessageIcons = new ImageSource[3];

        public MainWindow()
        {
            DOut.pl("in MainWindow constructor");

            InitializeComponent();

            SetupInteractionConsole();

            ViewModel = new PlotterViewModel(this);
            ViewModel.Open();

            PlotterViewModelProvider.Instance.Set(ViewModel);

            viewContainer.Focusable = true;

            ViewModel.ObjTreeVM.ObjectTree = ObjTree;

            ViewModel.SetupTextCommandView(textCommand);
            textCommand.Determine += TextCommand_OnDetermine;

            KeyDown += OnKeyDown;
            KeyUp += OnKeyUp;

            Loaded += MainWindow_Loaded;
            Closed += MainWindow_Closed;

            RunTextCommandButton.Click += RunTextCommandButtonClicked;

            SetupDataContext();

            InitWindowChrome();

            InitPopup();

            DOut.pl("out MainWindow constructor");
        }

        public CadConsoleView GetBuiltinConsole()
        {
            return MyConsole;
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

            ViewModePanel.DataContext = ViewModel.mViewManager;

            SnapMenu.DataContext = ViewModel.Settings;

            SettingsMenu.DataContext = ViewModel.Settings;

            DrawModeMenu.DataContext = ViewModel.Settings;

            DrawOptionMenu.DataContext = ViewModel.Settings;

            SnapToolBar.DataContext = ViewModel.Settings;

            TreeViewToolBar.DataContext = ViewModel.Settings;
        }

        private void InitWindowChrome()
        {
            BtnCloseWindow.Click += (sender, e) => { Close(); };
            BtnMinWindow.Click += (sender, e) => { this.WindowState = WindowState.Minimized; };
            BtnMaxWindow.Click += (sender, e) => { this.WindowState = WindowState.Maximized; };
            BtnRestWindow.Click += (sender, e) => { this.WindowState = WindowState.Normal; };

            StateChanged += MainWindow_StateChanged;
        }

        private void InitPopup()
        {
            InitPopupMessageIcons();
            PopupMessage.CustomPopupPlacementCallback = PlaceMessagePopup;
        }

        private void InitPopupMessageIcons()
        {
            PopupMessageIcons[(int)PlotterCallback.MessageType.INFO] =
                (ImageSource)TryFindResource("infoIconDrawingImage");

            PopupMessageIcons[(int)PlotterCallback.MessageType.INPUT] =
                (ImageSource)TryFindResource("inputIconDrawingImage");

            PopupMessageIcons[(int)PlotterCallback.MessageType.ERROR] =
                (ImageSource)TryFindResource("errorIconDrawingImage");
        }

        public CustomPopupPlacement[] PlaceMessagePopup(Size popupSize,
                                           Size targetSize,
                                           Point offset)
        {
            Point p = new Point(targetSize.Width - popupSize.Width - 8, 8);

            CustomPopupPlacement placement1 =
                new CustomPopupPlacement(p, PopupPrimaryAxis.Horizontal);

            CustomPopupPlacement[] ttplaces =
                    new CustomPopupPlacement[] { placement1 };
            return ttplaces;
        }

        ImageSource SelectPopupMessageIcon(PlotterCallback.MessageType type)
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
                            LayoutRoot.Margin = new Thickness(9);
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
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            DOut.pl("in MainWindow_Loaded");

            var hsrc = HwndSource.FromVisual(this) as HwndSource;
            hsrc.AddHook(WndProc);

            System.Drawing.Color c = ViewModel.DC.Tools.BrushColor(DrawTools.BRUSH_BACKGROUND);
            viewRoot.Background = new SolidColorBrush(Color.FromRgb(c.R, c.G, c.B));

            ImageRenderer.Provider.Get();

            DOut.pl("out MainWindow_Loaded");
        }

        private void Command_Clicked(object sender, RoutedEventArgs e)
        {
            Control control = (Control)sender;

            String command = control.Tag.ToString();

            ViewModel.ExecCommand(command);
        }

        #region TextCommand
        public void RunTextCommandButtonClicked(object sender, RoutedEventArgs e)
        {
            var s = textCommand.Text;

            textCommand.Text = "";

            if (s.Length > 0)
            {
                ViewModel.TextCommand(s);
                textCommand.History.Add(s);
                textCommand.Focus();
            }
        }

        private void TextCommand_OnDetermine(object sender, AutoCompleteTextBox.TextEventArgs e)
        {
            var s = e.Text;

            textCommand.Text = "";

            if (s.Length > 0)
            {
                ViewModel.TextCommand(s);
            }
        }
        #endregion

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

        public void SetPlotterView(IPlotterView view)
        {
            viewContainer.Child = view.FormsControl;
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
            }
            return IntPtr.Zero;
        }

        #region IMainWindow
        public Window GetWindow()
        {
            return this;
        }

        public void SetCurrentFileName(string file_name)
        {
            FileName.Content = file_name;
        }

        public void OpenPopupMessage(string text, PlotterCallback.MessageType messageType)
        {
            Application.Current.Dispatcher.Invoke(() => {
                if (PopupMessage.IsOpen)
                {
                    return;
                }

                PopupMessageIcon.Source = SelectPopupMessageIcon(messageType);

                PopupMessageText.Text = text;
                PopupMessage.IsOpen = true;
            });
        }

        public void ClosePopupMessage()
        {
            if (Application.Current.Dispatcher.Thread.ManagedThreadId ==
                Thread.CurrentThread.ManagedThreadId)
            {
                PopupMessage.IsOpen = false;
                return;
            }

            Application.Current.Dispatcher.Invoke(() => {
                PopupMessage.IsOpen = false;
            });
        }

        public void DrawModeUpdated(DrawTools.DrawMode drawMode)
        {
            System.Drawing.Color c = ViewModel.DC.Tools.BrushColor(DrawTools.BRUSH_BACKGROUND);
            viewRoot.Background = new SolidColorBrush(Color.FromRgb(c.R, c.G, c.B));
        }
        #endregion
    }
}
