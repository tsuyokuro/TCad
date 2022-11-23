using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TCad;

/// <summary>
/// PrintSettingsDialog.xaml の相互作用ロジック
/// </summary>
public partial class PrintSettingsDialog : Window
{
    public bool PrintWithBitmap;
    public double MagnificationBitmapPrinting;
    public bool PrintLineSmooth;

    public PrintSettingsDialog()
    {
        InitializeComponent();
        Loaded += PrintSettingsDialog_Loaded;
        PreviewKeyDown += PrintSettingsDialog_PreviewKeyDown;
        magnification_for_bitmap_printing.PreviewTextInput += PreviewTextInputForNum;
    }

    private void PrintSettingsDialog_PreviewKeyDown(object sender, KeyEventArgs e)
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

    private void PrintSettingsDialog_Loaded(object sender, RoutedEventArgs e)
    {
        print_with_bitmap.IsChecked = PrintWithBitmap;
        magnification_for_bitmap_printing.Text = MagnificationBitmapPrinting.ToString();
        print_line_smooth.IsChecked = PrintLineSmooth;

        ok_button.Click += Ok_button_Click;
        cancel_button.Click += Cancel_button_Click;
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

        PrintWithBitmap = print_with_bitmap.IsChecked.Value;

        ret &= Double.TryParse(magnification_for_bitmap_printing.Text, out v);
        MagnificationBitmapPrinting = v;

        PrintLineSmooth = print_line_smooth.IsChecked.Value;

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
