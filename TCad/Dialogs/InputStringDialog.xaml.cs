using System;
using System.Windows;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Media;

namespace TCad.Dialogs
{
    public partial class InputStringDialog : Window
    {
        public string Message
        {
            get
            {
                return (string)message.Content;
            }
            set
            {
                message.Content = value;
            }
        }


        public string InputString
        {
            get
            {
                return input.Text;
            }
            set
            {
                input.Text = value;
            }
        }

        public InputStringDialog()
        {
            InitializeComponent();
            MainWindow wnd = (MainWindow)Application.Current.MainWindow;

            //Point cp = new Point(0,0);
            Point cp = GetRightBottomPoint(wnd.viewContainer);

            Point p = wnd.viewContainer.PointToScreen(cp);

            Left = p.X;
            Top = p.Y;

            cancel_button.Click += Cancel_button_Click;
            ok_button.Click += Ok_button_Click;

            Loaded += InputStringDialog_Loaded;

            LayoutRoot.MouseLeftButtonDown += LayoutRoot_MouseLeftButtonDown;

            PreviewKeyDown += FigureNameDialog_PreviewKeyDown;
        }

        private void InputStringDialog_Loaded(object sender, RoutedEventArgs e)
        {
            input.Focus();
        }

        private Point GetRightBottomPoint(FrameworkElement view)
        {
            double x = view.ActualWidth - Width;
            double y = view.ActualHeight - Height;

            return new Point(x, y);
        }

        private void FigureNameDialog_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                HandleOK();
            }
            else if (e.Key == Key.Escape)
            {
                HandleCancel();
            }
        }

        private void LayoutRoot_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Ok_button_Click(object sender, RoutedEventArgs e)
        {
            HandleOK();
        }

        private void Cancel_button_Click(object sender, RoutedEventArgs e)
        {
            HandleCancel();
        }

        private void HandleOK()
        {
            this.DialogResult = true;
        }

        private void HandleCancel()
        {
            this.DialogResult = false;
        }
    }
}
