using AOEMods.Essence.SGA;
using Microsoft.Toolkit.Mvvm.Messaging;
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
                    new MemoryStream(file.GetData().ToArray()), file.Extension, file.Name
                ));
            }
        }

        private void OnOpenInChunkyViewerClicked(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem &&
                menuItem.Parent is ContextMenu contextMenu &&
                contextMenu.PlacementTarget is FrameworkElement element &&
                element.DataContext is ArchiveItemViewModel itemViewModel &&
                itemViewModel.Node is IArchiveFileNode file)
            {
                WeakReferenceMessenger.Default.Send(new OpenStreamMessage(
                    new MemoryStream(file.GetData().ToArray()), "chunky", file.Name
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

        private void OnAddFileClicked(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element &&
                element.DataContext is ArchiveItemViewModel itemViewModel)
            {
                itemViewModel.AddFileCommand.Execute(null);
            }
        }

        private void OnRenameClicked(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem &&
                menuItem.Parent is ContextMenu contextMenu &&
                contextMenu.PlacementTarget is FrameworkElement element &&
                element.DataContext is ArchiveItemViewModel itemViewModel)
            {
                itemViewModel.StartRenamingCommand.Execute(null);
            }
        }

        private void OnItemTextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element &&
                element.DataContext is ArchiveItemViewModel itemViewModel)
            {
                itemViewModel.CancelRenamingCommand.Execute(null);
            }
        }

        private void OnItemTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (sender is TextBox textBox &&
                textBox.DataContext is ArchiveItemViewModel itemViewModel)
            {
                switch (e.Key)
                {
                    case Key.Escape:
                        itemViewModel.CancelRenamingCommand.Execute(null);
                        break;
                    case Key.Enter:
                        textBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
                        itemViewModel.EndRenamingCommand.Execute(null);
                        break;
                }
            }
        }
    }
}
