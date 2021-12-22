using AOEMods.Essence.SGA;
using AOEMods.Essence.SGA.Graph;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace AOEMods.Essence.Editor
{
    public abstract class TreeViewTabItemViewModel : ObservableRecipient
    {
        public ObservableCollection<TreeViewTabItemViewModel>? Children
        {
            get => children;
            set => SetProperty(ref children, value);
        }

        private bool visibleChildOrSelf = true;

        public Visibility Visibility { 
            get => visibleChildOrSelf ? Visibility.Visible : Visibility.Collapsed;
        }

        private bool expanded;
        public bool IsExpanded
        {
            get => expanded;
            set {
                SetProperty(ref expanded, value);
                OnPropertyChanged(nameof(IsExpanded));
            }
        }

        protected ObservableCollection<TreeViewTabItemViewModel>? children = null;
        protected readonly TreeViewTabItemViewModel? parentViewModel = null;

        public TreeViewTabItemViewModel()
        {
            IsExpanded = false;
            visibleChildOrSelf = true;
        }

        public TreeViewTabItemViewModel(TreeViewTabItemViewModel? parentViewModel) : this()
        {
            this.parentViewModel = parentViewModel;
        }

        internal void ExpandCollapseAll(bool isExpanded)
        {
            this.IsExpanded = isExpanded;
            if (this.Children != null)
            {
                foreach (TreeViewTabItemViewModel item in this.Children)
                {
                    item.ExpandCollapseAll(isExpanded);
                }
            }
        }

        public bool CheckVisibility(string searchText)
        {
            var searchTextLower = searchText.ToLowerInvariant().Trim();
            if (String.IsNullOrWhiteSpace(searchTextLower))
            {
                visibleChildOrSelf = true;
            } 
            else
            {
                var visibleSelf = GetSearchTarget().Contains(searchTextLower);
                visibleChildOrSelf = visibleSelf;
            }

            if (Children != null) { 
                foreach (var child in Children)
                {
                    if (child != null)
                    {
                        visibleChildOrSelf |= child.CheckVisibility(searchTextLower);
                    }
                }
            }

            OnPropertyChanged(nameof(Visibility));
            return visibleChildOrSelf;
        }

        public abstract String GetSearchTarget();
    }
}
