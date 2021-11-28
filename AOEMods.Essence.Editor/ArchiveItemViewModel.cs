using AOEMods.Essence.SGA;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace AOEMods.Essence.Editor
{
    public class ArchiveItemViewModel : ObservableRecipient
    {
        public string? DisplayText => Node?.Name;

        public ObservableCollection<ArchiveItemViewModel>? Children
        {
            get => children;
            set => SetProperty(ref children, value);
        }

        public IArchiveNode? Node
        {
            get => node;
            set => SetProperty(ref node, value);
        }

        private IArchiveNode? node = null;

        private ObservableCollection<ArchiveItemViewModel>? children = null;
        private ArchiveItemViewModel? parentViewModel = null;

        public ICommand ExportCommand { get; }
        public ICommand DeleteCommand { get; }

        public ArchiveItemViewModel(IArchiveNode? node, ArchiveItemViewModel? parentViewModel)
        {
            Node = node;
            this.parentViewModel = parentViewModel;

            ExportCommand = new RelayCommand(Export);
            DeleteCommand = new RelayCommand(Delete);
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.PropertyName == nameof(Node))
            {
                if (Node != null && Node is IArchiveFolderNode folderNode)
                {
                    Children = new ObservableCollection<ArchiveItemViewModel>(folderNode.Children.Select(child => new ArchiveItemViewModel(child, this)));
                }
                else
                {
                    Children = null;
                }
            }
        }

        private void Export()
        {
            if (Node != null)
            {
                switch (Node)
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
                            VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog()
                            {
                                Description = "Select a directory to unpack the archive's folder into"
                            };

                            if (dialog.ShowDialog() == true)
                            {
                                void ExportRecursive(IArchiveNode node)
                                {
                                    if (node is IArchiveFileNode file)
                                    {
                                        byte[] data = file.GetData().ToArray();
                                        string relativePath = Path.GetRelativePath(Node.FullName, node.FullName);
                                        string outPath = Path.Join(dialog.SelectedPath, Node.Name, relativePath);
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

                                ExportRecursive(Node);
                            }
                            break;
                        }
                }
            }
        }

        private void Delete()
        {
            if (parentViewModel?.Node != null && Node != null)
            {
                parentViewModel.Children?.Remove(this);
                if (parentViewModel.Node is IArchiveFolderNode folderNode)
                {
                    folderNode.Children.Remove(Node);
                }
            }
        }
    }
}
