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
    /// Interaction logic for ExportFolderDialog.xaml
    /// </summary>
    public partial class ExportFolderDialog : Window
    {
        public ExportFolderDialogViewModel ViewModel => (ExportFolderDialogViewModel)DataContext;

        public ExportFolderDialog()
        {
            DataContext = new ExportFolderDialogViewModel();
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
