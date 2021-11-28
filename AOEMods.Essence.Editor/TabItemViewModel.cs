using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace AOEMods.Essence.Editor
{
    public abstract class TabItemViewModel : ObservableRecipient
    {
        public string TabTitle
        {
            get => tabTitle;
            set => SetProperty(ref tabTitle, value);
        }
        private string tabTitle = "";
    }
}
