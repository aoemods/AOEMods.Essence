using AOEMods.Essence.SGA;
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

        public Archive? Archive
        {
            get => archive;
            set => SetProperty(ref archive, value);
        }

        public override string TabTitle => Archive?.Name ?? "Unloaded archive";

        private Archive? archive = null;
        public ICommand PackCommand { get; }

        public ArchiveViewModel()
        {
            PackCommand = new RelayCommand(Pack);
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
