using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TCad;

public partial class GridSettingsDialog : Window
{
    public vector3_t GridSize = default;

    public GridSettingsDialog()
    {
        InitializeComponent();

        PreviewKeyDown += GridSettingsDialog_PreviewKeyDown;

        grid_x_size.PreviewTextInput += PreviewTextInputForNum;
        grid_y_size.PreviewTextInput += PreviewTextInputForNum;
        grid_z_size.PreviewTextInput += PreviewTextInputForNum;

        ok_button.Click += Ok_button_Click;
        cancel_button.Click += Cancel_button_Click;

        this.Loaded += GridSettingsDialog_Loaded;
    }

    private void GridSettingsDialog_PreviewKeyDown(object sender, KeyEventArgs e)
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

    private void GridSettingsDialog_Loaded(object sender, RoutedEventArgs e)
    {
        grid_x_size.Text = GridSize.X.ToString();
        grid_y_size.Text = GridSize.Y.ToString();
        grid_z_size.Text = GridSize.Z.ToString();
    }

    private void Cancel_button_Click(object sender, RoutedEventArgs e)
    {
        HandleCancel();
    }

    private void Ok_button_Click(object sender, RoutedEventArgs e)
    {
        HandleOK();
    }

    private void HandleOK()
    {
        bool ret = true;

        vcompo_t v;

        ret &= vcompo_t.TryParse(grid_x_size.Text, out v);
        GridSize.X = v;

        ret &= vcompo_t.TryParse(grid_y_size.Text, out v);
        GridSize.Y = v;

        ret &= vcompo_t.TryParse(grid_z_size.Text, out v);
        GridSize.Z = v;

        this.DialogResult = ret;
    }

    private void HandleCancel()
    {
        this.DialogResult = false;
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
