//#define DEFAULT_DATA_TYPE_DOUBLE
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


#if DEFAULT_DATA_TYPE_DOUBLE
using vcompo_t = System.Double;
using vector3_t = OpenTK.Mathematics.Vector3d;
using vector4_t = OpenTK.Mathematics.Vector4d;
using matrix4_t = OpenTK.Mathematics.Matrix4d;
#else
using vcompo_t = System.Single;
using vector3_t = OpenTK.Mathematics.Vector3;
using vector4_t = OpenTK.Mathematics.Vector4;
using matrix4_t = OpenTK.Mathematics.Matrix4;
#endif


namespace TCad;

public partial class SnapSettingsDialog : Window
{
    public vcompo_t PointSnapRange;
    public vcompo_t LineSnapRange;

    public SnapSettingsDialog()
    {
        InitializeComponent();

        point_snap.PreviewTextInput += PreviewTextInputForNum;
        line_snap.PreviewTextInput += PreviewTextInputForNum;

        ok_button.Click += Ok_button_Click;
        cancel_button.Click += Cancel_button_Click;

        PreviewKeyDown += SnapSettingsDialog_PreviewKeyDown;

        this.Loaded += SnapSettingsDialog_Loaded;
    }

    private void SnapSettingsDialog_PreviewKeyDown(object sender, KeyEventArgs e)
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

    private void SnapSettingsDialog_Loaded(object sender, RoutedEventArgs e)
    {
        point_snap.Text = PointSnapRange.ToString();
        line_snap.Text = LineSnapRange.ToString();
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

        ret &= vcompo_t.TryParse(point_snap.Text, out v);
        PointSnapRange = v;

        ret &= vcompo_t.TryParse(line_snap.Text, out v);
        LineSnapRange = v;

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
