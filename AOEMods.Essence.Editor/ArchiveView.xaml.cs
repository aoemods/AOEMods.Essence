using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Navigation;
using AOEMods.Essence.SGA;

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
            DataContext = new ArchiveViewModel();
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
            /*if (sender is MenuItem menuItem &&
                menuItem.Parent is ContextMenu contextMenu &&
                contextMenu.PlacementTarget is FrameworkElement element &&
                element.DataContext is ArchiveItemViewModel itemViewModel &&
                itemViewModel.Node is IArchiveFileNode file)
            {
                switch (file.Extension)
                {
                    case ".rgd":
                        var fileWindow = new GameDataWindow(file)
                        {
                            Title = file.FullName,
                        };
                        fileWindow.Show();
                        break;
                    case ".rrtex":
                        var textureView = new TextureWindow(file)
                        {
                            Title = file.FullName,
                        };
                        textureView.Show();
                        break;
                }
            }*/
        }
    }
}
