using Microsoft.Win32;
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

namespace AOEMods.Essence.Editor
{
    /// <summary>
    /// Interaction logic for ExportFileDialog.xaml
    /// </summary>
    public partial class ExportFileDialog : Window
    {
        public ExportFileDialogViewModel ViewModel => (ExportFileDialogViewModel)DataContext;

        public ExportFileDialog(object? exportOptionsViewModel)
        {
            DataContext = new ExportFileDialogViewModel()
            {
                ExportOptionsViewModel = exportOptionsViewModel
            };

            InitializeComponent();
        }

        private void OnExportClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void OnCancelClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
