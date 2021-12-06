using System;
using System.Windows;
using System.Windows.Input;

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

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            Application.Current.Shutdown();
        }

        private void OnTabItemMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle &&
                e.ButtonState == MouseButtonState.Pressed &&
                sender is FrameworkElement element &&
                element.DataContext is TabItemViewModel tabItemViewModel)
            {
                tabItemViewModel.CloseTabCommand.Execute(null);
                e.Handled = true;
            }
            else
            {
                e.Handled = false;
            }
        }
    }
}
