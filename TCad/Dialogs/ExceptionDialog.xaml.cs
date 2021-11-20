using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TCad
{
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
}
