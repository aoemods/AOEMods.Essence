using AOEMods.Essence.SGA;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Win32;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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
                switch (itemViewModel.Node)
                {
                    case IArchiveFileNode file:
                        {
                            SaveFileDialog saveFileDialog = new SaveFileDialog()
                            {
                                Filter = $"*.{file.Extension}|All(*.*)",
                                FileName = file.Name
                            };
                            if (saveFileDialog.ShowDialog() == true)
                            {
                                File.WriteAllBytes(saveFileDialog.FileName, file.GetData().ToArray());
                            }
                            break;
                        }
                    case IArchiveFolderNode:
                        {
                            var rootNode = itemViewModel.Node;
                            string folderPath = ".";

                            void ExportRecursive(IArchiveNode node)
                            {
                                if (node is IArchiveFileNode file)
                                {
                                    byte[] data = file.GetData().ToArray();
                                    string relativePath = Path.GetRelativePath(rootNode.FullName, node.FullName);
                                    string outPath = Path.Join(folderPath, rootNode.Name, relativePath);
                                    Directory.CreateDirectory(Path.GetDirectoryName(outPath));
                                    File.WriteAllBytes(outPath, data);
                                }
                                else if (node is IArchiveFolderNode folderNode)
                                {
                                    foreach (var childNodeChild in folderNode.Children)
                                    {
                                        ExportRecursive(childNodeChild);
                                    }
                                }
                            }

                            ExportRecursive(rootNode);
                            break;
                        }
                }
            }
        }

        private void OnDeleteClicked(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem &&
                menuItem.Parent is ContextMenu contextMenu &&
                contextMenu.PlacementTarget is FrameworkElement element &&
                element.DataContext is ArchiveItemViewModel itemViewModel)
            {
                itemViewModel.Delete();
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
    }
}
