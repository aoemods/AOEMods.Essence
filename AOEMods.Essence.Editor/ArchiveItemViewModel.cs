using AOEMods.Essence.SGA;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace AOEMods.Essence.Editor
{
    public class ArchiveItemViewModel : ObservableRecipient
    {
        public string? Name
        {
            get => name;
            set => SetProperty(ref name, value);
        }

        private string? name = null;

        public bool Renaming
        {
            get => renaming;
            private set => SetProperty(ref renaming, value);
        }

        public string RenamingName
        {
            get => renamingName;
            set => SetProperty(ref renamingName, value);
        }

        private string renamingName = "";

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
        private bool renaming;

        public ICommand ExportCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand OpenCommand { get; }
        public ICommand AddFileCommand { get; }
        public ICommand StartRenamingCommand { get; }
        public ICommand EndRenamingCommand { get; }
        public ICommand CancelRenamingCommand { get; }

        public ArchiveItemViewModel(IArchiveNode? node, ArchiveItemViewModel? parentViewModel)
        {
            Node = node;
            this.parentViewModel = parentViewModel;

            ExportCommand = new RelayCommand(Export);
            DeleteCommand = new RelayCommand(Delete);
            OpenCommand = new RelayCommand(Open);
            AddFileCommand = new RelayCommand(AddFile);
            StartRenamingCommand = new RelayCommand(StartRenaming);
            CancelRenamingCommand = new RelayCommand(CancelRenaming);
            EndRenamingCommand = new RelayCommand(EndRenaming);
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

                if (Node != null)
                {
                    Name = Node.Name;
                }
                else
                {
                    Name = null;
                }
            }

            if (e.PropertyName == nameof(Name) && Name != null && Node != null)
            {
                Node.Name = Name;
            }
        }

        private void Open()
        {
            if (node is IArchiveFileNode fileNode)
            {
                WeakReferenceMessenger.Default.Send(new OpenStreamMessage(
                    new MemoryStream(fileNode.GetData().ToArray()),
                    fileNode.Extension, fileNode.Name
                ));
            }
        }

        private void Export()
        {
            if (Node != null)
            {
                ExportArchiveUtil.ShowExportArchiveNodeDialog(
                    Node,
                    "Select a path to unpack to"
                );
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

        private void AddFile()
        {
            if (Node != null && Node is IArchiveFolderNode folderNode)
            {
                OpenFileDialog dialog = new OpenFileDialog()
                {
                    Title = "Select file to add to the archive",
                    Filter = "Essence files(*.sga, *.rgd, *.rrtex, *.rrgeom) | *.sga; *.rgd; *.rrtex; *.rrgeom | sga files(*.sga) | *.sga | rgd files(*.rgd) | *.rgd | rrtex files(*.rrtex) | *.rrtex | rrgeom files(*.rrgeom) | *.rrgeom | All files(*.*) | *.* "
                };

                if (dialog.ShowDialog() == true)
                {
                    IArchiveFileNode fileNode = new ArchiveStoredFileNode(Path.GetFileName(dialog.FileName), File.ReadAllBytes(dialog.FileName), folderNode);
                    folderNode.Children.Add(fileNode);

                    if (Children == null)
                    {
                        throw new Exception("Folder node view model Children was null");
                    }

                    Children.Add(new ArchiveItemViewModel(fileNode, this));
                }
            }
        }

        private void StartRenaming()
        {
            RenamingName = Name ?? "";
            Renaming = true;
        }

        private void CancelRenaming()
        {
            Renaming = false;
        }

        private void EndRenaming()
        {
            if (Renaming)
            {
                Name = RenamingName;
                Renaming = false;
            }
        }
    }
}
