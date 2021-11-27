using AOEMods.Essence.SGA;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

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

        public ArchiveItemViewModel(IArchiveNode? node, ArchiveItemViewModel? parentViewModel)
        {
            Node = node;
            this.parentViewModel = parentViewModel;
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

        public void Delete()
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
