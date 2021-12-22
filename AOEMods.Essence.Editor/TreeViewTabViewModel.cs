using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace AOEMods.Essence.Editor
{
    public abstract class TreeViewTabViewModel : TabViewModel
    {
        public ObservableCollection<TreeViewTabItemViewModel>? RootChildren
        {
            get => rootChildren;
            set => SetProperty(ref rootChildren, value);
        }

        private ObservableCollection<TreeViewTabItemViewModel>? rootChildren = null;

        public ICommand ExpandCollapseAllCommand { get; protected set; }

        private bool expanded;
        public bool IsExpanded
        {
            get => expanded;
            set => SetProperty(ref expanded, value);
        }

        private string searchText = string.Empty;
        public string SearchText
        {
            get => searchText;
            set
            {
                SetProperty(ref searchText, value);
                OnPropertyChanged(nameof(SearchText));
            }
        }

        protected TreeViewTabViewModel() : base()
        {
            ExpandCollapseAllCommand = new RelayCommand(ExpandCollapseAll);
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.PropertyName == nameof(SearchText))
            {
                OnSearchTextChanged();
            }
        }

        private void OnSearchTextChanged()
        {
            var searchResult = false;
            if (rootChildren != null)
            {
                foreach (var item in rootChildren)
                {
                    if (item != null)
                    {
                        searchResult = item.CheckVisibility(searchText) ? true : searchResult;
                    }
                }
            }

            if (searchResult)
            {
                IsExpanded = true;
                ExpandCollapseAll(true);
            }
        }

        public void ExpandCollapseAll()
        {
            IsExpanded = !IsExpanded;
            ExpandCollapseAll(IsExpanded);
        }

        private void ExpandCollapseAll(bool flag)
        {
            if (RootChildren != null)
            {
                foreach (var child in RootChildren)
                {
                    child?.ExpandCollapseAll(IsExpanded);
                }
            }
        }
    }
}
