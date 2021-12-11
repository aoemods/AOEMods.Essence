using AOEMods.Essence.SGA.Core;
using AOEMods.Essence.SGA.Graph;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Win32;
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

        private IArchive? archive = null;
        public ICommand PackCommand { get; }
        public ICommand UnpackCommand { get; }
        public ICommand OpenPropertiesCommand { get; }

        private ArchivePropertiesView? propertiesWindow = null;

        public ArchiveViewModel()
        {
            PackCommand = new RelayCommand(Pack);
            UnpackCommand = new RelayCommand(Unpack);
            OpenPropertiesCommand = new RelayCommand(OpenProperties);
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
                    ArchiveWriterHelper.WriteArchiveToStream(outFile, Archive);
                }
            }
        }

        private void Unpack()
        {
            if (Archive != null)
            {
                ExportArchiveUtil.ShowExportArchiveNodeDialog(
                    Archive.Tocs[0].RootFolder,
                    "Select a directory to unpack the archive into"
                );
            }
        }

        private void OpenProperties()
        {
            if (Archive != null)
            {
                if (propertiesWindow == null || !propertiesWindow.IsLoaded)
                {
                    propertiesWindow = new ArchivePropertiesView();
                }
                else
                {
                    propertiesWindow.Activate();
                }

                propertiesWindow.ViewModel.Archive = Archive;
                propertiesWindow.Show();
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
