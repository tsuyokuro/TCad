using System.Windows;

namespace TCad;

public partial class EceptionDialog : Window
{
    public EceptionDialog()
    {
        InitializeComponent();

        btnContinue.Click += BtnContinue_Click;
        btnStop.Click += BtnStop_Click;
    }

    private void BtnContinue_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }

    private void BtnStop_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
