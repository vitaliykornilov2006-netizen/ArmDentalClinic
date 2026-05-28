using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ARMDental.Views
{
    public partial class ReportsDialog
    {
        public ReportsDialog()
        {
            InitializeComponent();
        }

        private void btnGenerate_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ReportSelectWindow();
            dialog.Owner = Application.Current.MainWindow;
            dialog.ShowDialog();
        }
    }
}
