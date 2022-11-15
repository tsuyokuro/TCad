using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TCad;

public partial class MoveKeySettingsDialog : Window
{
    public double MoveX;
    public double MoveY;

    public MoveKeySettingsDialog()
    {
        InitializeComponent();

        move_x.PreviewTextInput += PreviewTextInputForNum;
        move_y.PreviewTextInput += PreviewTextInputForNum;

        ok_button.Click += Ok_button_Click;
        cancel_button.Click += Cancel_button_Click;

        PreviewKeyDown += MoveKeySettingsDialog_PreviewKeyDown;

        this.Loaded += MoveKeySettingsDialog_Loaded;
    }

    private void MoveKeySettingsDialog_PreviewKeyDown(object sender, KeyEventArgs e)
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

    private void MoveKeySettingsDialog_Loaded(object sender, RoutedEventArgs e)
    {
        move_x.Text = MoveX.ToString();
        move_y.Text = MoveY.ToString();
    }

    private void Cancel_button_Click(object sender, RoutedEventArgs e)
    {
        HandleCancel();
    }

    private void Ok_button_Click(object sender, RoutedEventArgs e)
    {
        HandleOK();
    }

    private void HandleCancel()
    {
        DialogResult = false;
    }

    private void HandleOK()
    {
        bool ret = true;

        double v;

        ret &= Double.TryParse(move_x.Text, out v);
        MoveX = v;

        ret &= Double.TryParse(move_y.Text, out v);
        MoveY = v;

        DialogResult = ret;
    }

    private void PreviewTextInputForNum(object sender, TextCompositionEventArgs e)
    {
        bool ok = false;

        TextBox tb = (TextBox)sender;

        double v;
        var tmp = tb.Text + e.Text;
        ok = Double.TryParse(tmp, out v);

        e.Handled = !ok;
    }
}
