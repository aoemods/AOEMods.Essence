using System.Windows;

namespace AOEMods.Essence.Editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainViewModel ViewModel => (MainViewModel)DataContext;

        public MainWindow()
        {
            DataContext = new MainViewModel();
            InitializeComponent();
        }

        private void OnDrop(object sender, DragEventArgs e)
        {
            ViewModel.OnDrop(e.Data);
        }
    }
}
