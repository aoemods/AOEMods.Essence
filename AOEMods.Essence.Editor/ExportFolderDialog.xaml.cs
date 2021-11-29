using System.Windows;

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
