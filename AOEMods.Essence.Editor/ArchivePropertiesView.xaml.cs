using System.Windows;

namespace AOEMods.Essence.Editor
{
    /// <summary>
    /// Interaction logic for ArchivePropertiesView.xaml
    /// </summary>
    public partial class ArchivePropertiesView : Window
    {
        public ArchivePropertiesViewModel ViewModel => (ArchivePropertiesViewModel)DataContext;

        public ArchivePropertiesView()
        {
            DataContext = new ArchivePropertiesViewModel();
            InitializeComponent();
        }

        private void OnOkClicked(object sender, RoutedEventArgs e)
        {
            ViewModel.ApplyCommand.Execute(null);
            Close();
        }

        private void OnCancelClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
