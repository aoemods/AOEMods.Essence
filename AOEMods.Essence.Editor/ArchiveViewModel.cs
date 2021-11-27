using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using AOEMods.Essence.SGA;

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

        public ArchiveViewModel()
        {
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.PropertyName == nameof(Archive))
            {
                if (archive != null)
                {
                    RootChildren = new ObservableCollection<ArchiveItemViewModel>(
                        archive.Tocs.Select(child => new ArchiveItemViewModel(child, null))
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
