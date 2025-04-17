using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
