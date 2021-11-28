using AOEMods.Essence.SGA;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Win32;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AOEMods.Essence.Editor
{
    /// <summary>
    /// Interaction logic for ArchiveView.xaml
    /// </summary>
    public partial class ArchiveView : UserControl
    {
        public ArchiveViewModel ViewModel => (ArchiveViewModel)DataContext;

        public ArchiveView()
        {
            InitializeComponent();
        }

        private void OnExportClicked(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem &&
                menuItem.Parent is ContextMenu contextMenu &&
                contextMenu.PlacementTarget is FrameworkElement element &&
                element.DataContext is ArchiveItemViewModel itemViewModel)
            {
                itemViewModel.ExportCommand.Execute(null);
            }
        }

        private void OnDeleteClicked(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem &&
                menuItem.Parent is ContextMenu contextMenu &&
                contextMenu.PlacementTarget is FrameworkElement element &&
                element.DataContext is ArchiveItemViewModel itemViewModel)
            {
                itemViewModel.DeleteCommand.Execute(null);
            }
        }

        private void OnOpenClicked(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem &&
                menuItem.Parent is ContextMenu contextMenu &&
                contextMenu.PlacementTarget is FrameworkElement element &&
                element.DataContext is ArchiveItemViewModel itemViewModel &&
                itemViewModel.Node is IArchiveFileNode file)
            {
                WeakReferenceMessenger.Default.Send(new OpenStreamMessage(
                    new MemoryStream(file.GetData().ToArray()), file.Extension
                ));
            }
        }

        private void OnItemDoubleClicked(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element &&
                element.DataContext is ArchiveItemViewModel itemViewModel)
            {
                itemViewModel.OpenCommand.Execute(null);
            }
        }
    }
}
