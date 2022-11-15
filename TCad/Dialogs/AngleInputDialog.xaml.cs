using System;
using System.Windows;
using System.Windows.Input;

namespace TCad.Dialogs;

public partial class AngleInputDialog : Window
{
    public AngleInputDialog()
    {
        InitializeComponent();
        MainWindow wnd = (MainWindow)Application.Current.MainWindow;

        Point p = wnd.viewContainer.PointToScreen(new Point(0, 0));

        this.Left = p.X;
        this.Top = p.Y;

        cancel_button.Click += Cancel_button_Click;
        ok_button.Click += Ok_button_Click;

        LayoutRoot.MouseLeftButtonDown += LayoutRoot_MouseLeftButtonDown;

        PreviewKeyDown += AngleInputDialog_PreviewKeyDown;
    }

    private void AngleInputDialog_PreviewKeyDown(object sender, KeyEventArgs e)
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

    public double GetDouble()
    {
        string s = input.Text;
        double v;
        Double.TryParse(s, out v);

        return v;
    }

    public string GetInputString()
    {
        string s = input.Text;
        return s;
    }
}
