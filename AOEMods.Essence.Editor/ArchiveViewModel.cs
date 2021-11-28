using AOEMods.Essence.SGA;
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
    public class ArchiveViewModel : TabItemViewModel
    {
        public ObservableCollection<ArchiveItemViewModel>? RootChildren
        {
            get => rootChildren;
            set => SetProperty(ref rootChildren, value);
        }

        private ObservableCollection<ArchiveItemViewModel>? rootChildren = null;

        public IArchive? Archive
        {
            get => archive;
            set => SetProperty(ref archive, value);
        }

        public override string TabTitle => Archive?.Name ?? "Unloaded archive";

        private IArchive? archive = null;
        public ICommand PackCommand { get; }
        public ICommand UnpackCommand { get; }

        public ArchiveViewModel()
        {
            PackCommand = new RelayCommand(Pack);
            UnpackCommand = new RelayCommand(Unpack);
        }

        private void Pack()
        {
            if (Archive != null)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog()
                {
                    Filter = $"sga files (*.sga)|*.sga|All files (*.*)|*.*",
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    using var outFile = File.Open(saveFileDialog.FileName, FileMode.Create, FileAccess.Write);
                    var archiveWriter = new ArchiveWriter(outFile);
                    archiveWriter.Write(Archive);
                }
            }
        }

        private void Unpack()
        {
            if (Archive != null)
            {
                VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog()
                {
                    Description = "Select a directory to unpack the archive into"
                };

                if (dialog.ShowDialog() == true)
                {
                    var fileNodes = ArchiveNodeHelper.EnumerateChildren(Archive.Tocs[0].RootFolder).OfType<IArchiveFileNode>();

                    foreach (var fileNode in fileNodes)
                    {
                        string fullName = fileNode.FullName;
                        var outputFilePath = Path.Join(dialog.SelectedPath, fullName);
                        var outputDirectory = Path.GetDirectoryName(outputFilePath);
                        if (outputDirectory != null)
                        {
                            Directory.CreateDirectory(outputDirectory);
                        }
                        File.WriteAllBytes(outputFilePath, fileNode.GetData().ToArray());
                    }
                }
            }
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.PropertyName == nameof(Archive))
            {
                if (archive != null && archive.Tocs.Count > 0)
                {
                    RootChildren = new ObservableCollection<ArchiveItemViewModel>(
                        archive.Tocs[0].RootFolder.Children.Select(
                            child => new ArchiveItemViewModel(child, null)
                        )
                    );
                }
                else
                {
                    RootChildren = null;
                }
            }
        }
    }
}
