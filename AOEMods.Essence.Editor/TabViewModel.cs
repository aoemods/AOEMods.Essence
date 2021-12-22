using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using System.Windows.Input;

namespace AOEMods.Essence.Editor
{
    public abstract class TabViewModel : ObservableRecipient
    {
        public string TabTitle
        {
            get => tabTitle;
            set => SetProperty(ref tabTitle, value);
        }
        private string tabTitle = "";

        public ICommand CloseTabCommand { get; }

        protected TabViewModel()
        {
            CloseTabCommand = new RelayCommand(CloseTab);
        }

        private void CloseTab()
        {
            WeakReferenceMessenger.Default.Send(new CloseTabMessage(this));
        }
    }
}
