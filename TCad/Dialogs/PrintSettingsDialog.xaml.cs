using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using vcompo_t = System.Single;
using vector3_t = OpenTK.Mathematics.Vector3;
using vector4_t = OpenTK.Mathematics.Vector4;
using matrix4_t = OpenTK.Mathematics.Matrix4;

namespace TCad;

/// <summary>
/// PrintSettingsDialog.xaml の相互作用ロジック
/// </summary>
public partial class PrintSettingsDialog : Window
{
    public bool PrintWithBitmap;
    public vcompo_t MagnificationBitmapPrinting;
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

        vcompo_t v;

        PrintWithBitmap = print_with_bitmap.IsChecked.Value;

        ret &= vcompo_t.TryParse(magnification_for_bitmap_printing.Text, out v);
        MagnificationBitmapPrinting = v;

        PrintLineSmooth = print_line_smooth.IsChecked.Value;

        DialogResult = ret;
    }

    private void PreviewTextInputForNum(object sender, TextCompositionEventArgs e)
    {
        bool ok = false;

        TextBox tb = (TextBox)sender;

        vcompo_t v;
        var tmp = tb.Text + e.Text;
        ok = vcompo_t.TryParse(tmp, out v);

        e.Handled = !ok;
    }
}
